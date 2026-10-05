#!/usr/bin/env python3
"""Static sensitivity model; this is not evidence of a Unity playthrough.

Read the existing MAP1 route/pads from C# and model one isolated boss against
the existing five L3 Frost towers. Damage, speed, projectile speed, slow cap,
duration and cooldown remain unchanged. Only hypothetical L3 range varies.
"""
import argparse
import bisect
import hashlib
import json
import math
from pathlib import Path
import re


def read_geometry(project):
    path = project / 'UnityProject/Assets/FantasyTD/Scripts/MapAData.cs'
    content = path.read_text(encoding='utf-8')
    vector = r'new Vector2\(([\d.]+)f,\s*([\d.]+)f\)'
    route = [tuple(map(float, point)) for point in re.findall(
        vector, content.split('Waypoints =')[1].split('};')[0])]
    spots = {name: (float(x), float(y)) for name, x, y in re.findall(
        r'PlantingSpotDefinition\("([^\"]+)".*?' + vector, content)}
    return route, spots, hashlib.sha256(path.read_bytes()).hexdigest()


def route_distances(route):
    lengths = [math.dist(a, b) for a, b in zip(route, route[1:])]
    cumulative = [0.0]
    for length in lengths:
        cumulative.append(cumulative[-1] + length)
    return lengths, cumulative


def position_at(distance, route, lengths, cumulative):
    index = min(len(lengths) - 1, bisect.bisect_right(cumulative, distance) - 1)
    fraction = (distance - cumulative[index]) / lengths[index]
    a, b = route[index:index + 2]
    return tuple(a[axis] + (b[axis] - a[axis]) * fraction for axis in (0, 1))


def minimum_route_distance(point, route):
    result = math.inf
    for a, b in zip(route, route[1:]):
        dx, dy = b[0] - a[0], b[1] - a[1]
        fraction = max(0.0, min(1.0,
            ((point[0] - a[0]) * dx + (point[1] - a[1]) * dy) / (dx * dx + dy * dy)))
        result = min(result, math.dist(point, (a[0] + fraction * dx, a[1] + fraction * dy)))
    return result


def simulate(route, spots, plan, step, ranged_radius):
    lengths, cumulative = route_distances(route)
    identifiers = list(plan)
    cooldowns = [1.0 if plan[name] == 'Oak' else 0.0 for name in identifiers]
    projectiles = []
    shots = {name: 0 for name in identifiers}
    damage_by_tower = {name: 0 for name in identifiers}
    time = progress = damage = slow_until = root_until = 0.0
    kill_time = kill_progress = None
    damage_on_killing_hit = None
    # Continue a counterfactual alive target to the base to measure available
    # damage budget. Actual gameplay stops its attacks on the real damage kill.
    while progress < cumulative[-1] and time < 240.0:
        time += step
        multiplier = 0.0 if time < root_until else (0.8 if time < slow_until else 1.0)
        progress += 0.3 * multiplier * step
        if progress >= cumulative[-1]:
            break
        target = position_at(progress, route, lengths, cumulative)
        for index, name in enumerate(identifiers):
            kind = plan[name]
            cooldowns[index] -= step
            radius = 2.4 if kind == 'Oak' else ranged_radius
            if cooldowns[index] > 1e-8 or math.dist(spots[name], target) > radius:
                continue
            shots[name] += 1
            if kind == 'Oak':
                damage += 16
                damage_by_tower[name] += 16
                root_until = max(root_until, time + 2.1 * 0.35)
                cooldowns[index] = 4.5
            else:
                # Seed physical damage is reduced by boss resistance; Frost is not.
                projectile_damage = 40 * 0.8 if kind == 'Seed' else 30
                projectiles.append([*spots[name], projectile_damage, kind == 'Frost', name])
                cooldowns[index] = 0.85 if kind == 'Seed' else 0.9
        remaining = []
        for px, py, projectile_damage, frost, name in projectiles:
            distance = math.dist((px, py), target)
            travel = min(distance, 7 * step)
            if distance:
                px += (target[0] - px) * travel / distance
                py += (target[1] - py) * travel / distance
            if math.dist((px, py), target) <= 0.10:
                damage += projectile_damage
                damage_by_tower[name] += projectile_damage
                if frost:
                    slow_until = max(slow_until, time + 2.5)
            else:
                remaining.append([px, py, projectile_damage, frost, name])
        projectiles = remaining
        if damage >= 2200 and kill_time is None:
            kill_time, kill_progress, damage_on_killing_hit = time, progress, damage
    return {
        'gameStepSeconds': step,
        'hypotheticalL3Range': ranged_radius,
        'counterfactualFullRouteDamageBudget': int(damage),
        'budgetMarginAbove2200Hp': int(damage - 2200),
        'predictedDamageKill': kill_time is not None,
        'predictedKillGameSeconds': None if kill_time is None else round(kill_time, 3),
        'predictedKillRouteProgressRu': None if kill_progress is None else round(kill_progress, 3),
        'damageOnKillingFrame': damage_on_killing_hit,
        'counterfactualTravelGameSeconds': round(time, 3),
        'counterfactualShots': shots,
        'counterfactualDamageByTower': damage_by_tower,
    }


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--project', type=Path, default=Path(__file__).resolve().parents[2])
    parser.add_argument('--output', type=Path)
    args = parser.parse_args()
    route, spots, geometry_sha = read_geometry(args.project)
    plan = {name: 'Frost' for name in ['A1', 'A5', 'A9', 'A11', 'A12']}
    minimums = {name: round(minimum_route_distance(point, route), 6) for name, point in spots.items()}
    report = {
        'status': 'STATIC_MODEL_ONLY_UNITY_UNVERIFIED',
        'geometrySourceSha256': geometry_sha,
        'plan': plan,
        'unchangedParameters': {
            'bossHp': 2200, 'bossSpeed': 0.3, 'bossPhysicalResistance': 0.2,
            'bossSlowCap': 0.2, 'bossRootDurationMultiplier': 0.35,
            'frostL3Damage': 30, 'frostL3Cooldown': 0.9,
            'slowDuration': 2.5, 'projectileSpeed': 7, 'hitDistance': 0.1,
        },
        'assumptions': [
            'Single isolated boss; no competing wave targets.',
            'All five towers already purchased and genuinely upgraded to L3.',
            'Movement, tower updates, then projectiles; newborn projectiles update in the same modeled frame.',
            'Fine step 0.005 game seconds approximates timing; it is not an exact Unity simulation.',
            '4x/60fps uses 0.0666667 game seconds per frame and unchanged production cooldown assignment.',
            'Budget continues a counterfactual alive target after 2200 damage; real gameplay stops on the actual kill.',
            'No extra pad, currency, damage, victory or boss-stat changes are applied to the project.',
        ],
        'minimumPadDistanceToRouteRu': minimums,
        'rangeCases': [],
    }
    for radius in (3.2, 3.4, 3.5, 3.6):
        report['rangeCases'].append({
            'rangeRu': radius,
            'newAttackReachablePadsVs3_2': [name for name, distance in minimums.items() if 3.2 < distance <= radius],
            'fineStep': simulate(route, spots, plan, 0.005, radius),
            'fourTimesAt60Fps': simulate(route, spots, plan, 4 / 60, radius),
        })
    encoded = json.dumps(report, indent=2) + '\n'
    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(encoded, encoding='utf-8')
    else:
        print(encoded, end='')


if __name__ == '__main__':
    main()
