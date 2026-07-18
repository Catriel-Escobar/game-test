# Estructura para implementar Animator del player

## Objetivo

Separar la logica de movimiento, input y animacion para que el sistema sea escalable y facil de mantener.

## Estructura de carpetas sugerida

```text
Assets
├── Scripts
│   ├── Core
│   │   ├── DamageData.cs
│   │   ├── IEntity.cs
│   │   └── IDamageable.cs
│   ├── Player
│   │   ├── Player.cs
│   │   ├── PlayerInputs.cs
│   │   ├── PlayerMovement.cs
│   │   ├── PlayerResources.cs
│   │   ├── PlayerStats.cs
│   │   ├── PlayerProgression.cs
│   │   ├── PlayerAnimationController.cs
│   │   ├── PlayerAnimatorParameters.cs
│   │   ├── PlayerStateMachine.cs
│   │   └── States
│   │       ├── PlayerIdleState.cs
│   │       ├── PlayerMoveState.cs
│   │       ├── PlayerJumpState.cs
│   │       ├── PlayerFallState.cs
│   │       ├── PlayerHitState.cs
│   │       └── PlayerDeathState.cs
│   └── UI
│       └── Player
│           └── PlayerResourcesUI.cs
├── Animations
│   └── Player
│       ├── Clips
│       │   ├── Idle.anim
│       │   ├── Walk.anim
│       │   ├── Run.anim
│       │   ├── Jump.anim
│       │   ├── Fall.anim
│       │   ├── Hit.anim
│       │   └── Death.anim
│       ├── Controllers
│       │   └── Player.controller
│       └── Overrides
│           └── PlayerOverride.controller
├── Prefabs
│   └── Player
│       └── Player.prefab
├── Art
│   └── Characters
└── Input
    └── InputSystem_Actions.inputactions
```

## Responsabilidad de cada capa

### PlayerMovement

- Mueve al personaje con CharacterController.
- Maneja rotacion, gravedad y estado fisico basico.
- No deberia conocer detalles del Animator.

### PlayerInputs

- Lee el input.
- Envuelve la entrada y la envia al sistema de movimiento o estado.
- No deberia decidir animaciones directamente.

### PlayerAnimationController

- Lee el estado del jugador.
- Actualiza parametros del Animator.
- Escucha eventos como muerte o damage si hace falta.

### PlayerResources

- Maneja vida y mana.
- Expone eventos como muerte o cambio de salud.

### Player

- Actua como fachada.
- Reune referencias a los componentes del player.
- Facilita el wiring inicial desde bootstrap.

### PlayerStateMachine

- Opcional, pero recomendable si el jugador va a tener varios estados.
- Ayuda a escalar sin llenar scripts de if y flags.

## Parametros recomendados para el Animator

```text
Float  Speed
Float  MoveX
Float  MoveY
Float  VerticalVelocity
Bool   IsGrounded
Bool   IsMoving
Bool   IsSprinting
Trigger Jump
Trigger Hit
Trigger Death
```

## Flujo recomendado

1. `PlayerInputs` lee el input.
2. `PlayerMovement` calcula el desplazamiento real.
3. `PlayerAnimationController` recibe ese estado y actualiza el Animator.
4. `PlayerResources` dispara eventos de vida o muerte.
5. Si el proyecto crece, `PlayerStateMachine` coordina estados de gameplay.

## Prompt para otra IA

Necesito que me ayudes a implementar un sistema de animacion para el player en Unity.

El proyecto ya tiene esta base:

- `PlayerInputs` lee el input.
- `PlayerMovement` mueve al personaje con `CharacterController`.
- `PlayerResources` maneja vida y mana.
- `Player` actua como fachada.
- Los mobs ya usan una maquina de estados, asi que quiero una solucion escalable y limpia para el player.

Quiero que propongas una arquitectura para el Animator que cumpla esto:

- No mezclar logica de animacion dentro de `PlayerMovement`.
- Usar un componente separado para controlar `Animator`.
- Poder escalar despues a `idle`, `walk`, `run`, `jump`, `fall`, `hit` y `death`.
- Mantener el codigo simple y desacoplado.
- Si hace falta, sugerir una state machine para el player.

Necesito que me digas:

- Que scripts nuevos crear.
- Que parametros deberia tener el Animator.
- Como conectar `PlayerInputs`, `PlayerMovement` y `PlayerResources` con la animacion.
- Que responsabilidades deberia tener cada clase.
- Un ejemplo de implementacion base.

## Recomendacion final

Si quiero una solucion simple ahora, creo un `PlayerAnimationController` separado.

Si quiero una solucion mas robusta para futuro, agrego una `PlayerStateMachine` y dejo la animacion como una capa de presentacion que solo refleja el estado actual.