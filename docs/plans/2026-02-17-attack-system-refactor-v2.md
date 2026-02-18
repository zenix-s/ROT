# Attack System Refactor v2: Resource como Orquestador — Plan de Implementación

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Mover la orquestación del ciclo de vida de los ataques al Resource (`AttackData.Spawn()`), convirtiendo las scenes en contenedores visuales tontos.

**Architecture:** `AttackData` gana un método virtual `Spawn(AttackContext)` que maneja instanciación, posicionamiento, hitbox init. `ProjectileData` override añade movimiento + lifetime. Nuevo `BurstAttackData` reemplaza `IceShard.cs`. Proyectiles pasan de `CharacterBody2D` a `Area2D` con movimiento en `AttackMovementComponent._PhysicsProcess()`. `IAttack` interface se elimina.

**Tech Stack:** Godot 4.6, C# (.NET 10.0), Resources, Area2D

**Verificación:** `dotnet build` + testeo manual en Godot (F5). No hay test suite automatizada.

---

## Task 1: Crear AttackContext record

**Files:**
- Create: `Core/Combat/Attacks/AttackContext.cs`

## Task 2: Añadir Spawn() virtual a AttackData

**Files:**
- Modify: `Core/Combat/Attacks/AttackData.cs`

## Task 3: Refactorizar AttackMovementComponent + LinearMovementComponent

**Files:**
- Modify: `Core/Combat/Components/AttackMovementComponents/AttackMovementComponent.cs`
- Modify: `Core/Combat/Components/AttackMovementComponents/LinearMovementComponent.cs`

## Task 4: Añadir Spawn() override a ProjectileData

**Files:**
- Modify: `Core/Combat/Attacks/ProjectileData.cs`

## Task 5: Convertir Projectile.tscn a Area2D, eliminar Projectile.cs

**Files:**
- Delete: `Scenes/Attacks/Projectiles/Projectile.cs`
- Rewrite: `Scenes/Attacks/Projectiles/Projectile.tscn`

## Task 6: Simplificar Fireball (eliminar script, YAGNI)

**Files:**
- Delete: `Scenes/Attacks/Projectiles/Fireball/Fireball.cs`
- Rewrite: `Scenes/Attacks/Projectiles/Fireball/FireBall.tscn`

## Task 7: Crear BurstAttackData, eliminar IceShard, actualizar .tres

**Files:**
- Create: `Core/Combat/Attacks/BurstAttackData.cs`
- Delete: `Scenes/Attacks/Spells/IceShard/IceShard.cs`
- Delete: `Scenes/Attacks/Spells/IceShard/IceShard.tscn`
- Modify: `Resources/Attacks/IceShardProjectile.tres`
- Rewrite: `Resources/Attacks/IceShard.tres`

## Task 8: Simplificar RockBody (quitar script)

**Files:**
- Delete: `Scenes/Attacks/Body/RockBody/RockBody.cs`
- Rewrite: `Scenes/Attacks/Body/RockBody/RockBody.tscn`

## Task 9: Simplificar AttackManagerComponent y eliminar IAttack

**Files:**
- Modify: `Core/Combat/Components/AttackManagerComponent.cs`
- Delete: `Core/Combat/Attacks/IAttack.cs`

## Task 10: Cleanup legacy .tres y verificación final

**Files:**
- Delete: `Scenes/Attacks/Projectiles/ProjectileAttackData.tres`
- Delete: `Scenes/Attacks/Projectiles/Fireball/FireballAttackData.tres`

See conversation context for full implementation details per task.
