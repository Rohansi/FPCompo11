include 'glitch.inc'

jmp entryPoint
rb 8 - ($-$$)

;
; Variable locations to corrupt
;

variableCount:
    dd 5

varTurnSpeed:
    db VAR_SPEED
    dd turnSpeed

varThrustSpeed:
    db VAR_SPEED
    dd thrustSpeed

varReverseDist:
    db VAR_GENERAL
    dd reverseDist

varNoThrustDiff:
    db VAR_GENERAL
    dd noThrustDiff

varTargetType:
    db VAR_RADARVALUE
    dd targetType

;
; Variable declarations
;

turnSpeed:
    dd 65

thrustSpeed:
    dd 75

reverseDist:
    dd 25

noThrustDiff:
    dd 15

targetType:
    dd RADAR_ENEMY

targetDir:
    dd RADAR_INVALID

;
; Ship code
;

entryPoint:
    ; enables interrupts
    ivt interruptTable
    sti

    ; enable radar
    mov r7, radarData
    int DEV_RADAR

main:
    cmp [targetDir], RADAR_INVALID
    jne .logic

    ; no target, turn off everything
    xor r7, r7
    xor r8, r8
    int DEV_ENGINES
    int DEV_GUNS
    inc r7
    int DEV_ENGINES
    jmp main
    
.logic:
    mov r7, 2
    int DEV_NAVIGATION
    mov r0, r7
    push r0     ; r0 = heading

.checkTurn:
    cmp r0, [targetDir]
    je .facingTarget
    mov r1, [targetDir]
    call closestDirection
    mov r8, [turnSpeed]
    mul r8, r2
    jmp .setTurn
.facingTarget:
    xor r8, r8
.setTurn:
    mov r7, 1
    int DEV_ENGINES

    pop r0
    mov r5, r0
    sub r5, [targetDir]
    abs r5
    mul r0, 2

.checkThrust:
    xor r8, r8
    cmp r5, [noThrustDiff]
    jae .setThrust
.shouldThrust:
    mov r8, [thrustSpeed]
    cmp byte [r0 + radarData + 1], [reverseDist]
    ja .setThrust
    neg r8
.setThrust:
    xor r7, r7
    int DEV_ENGINES

.checkShoot:
    cmp byte [r0 + radarData + 0], [targetType]
    jne .noShoot
    inc r7
.noShoot:
    int DEV_GUNS

    jmp main
 
; Returns the direction to spin to reach a target angle the fastest
; r0 - current heading
; r1 - target heading
; r2 - return value
closestDirection:
    push r0
    push r3 ; temp
    push r4 ; temp
    
    mov r2, 1
    xor r3, r3
    xor r4, r4
    sub r0, r1
    cmp r0, 0
    jbe .below0
    inc r3
.below0:
    abs r0
    cmp r0, RADAR_RAYCOUNT / 2
    jbe .below100
    inc r4
.below100:
    xor r3, r4
    jz .default
    mov r2, -1
.default:
 
    pop r4
    pop r3
    pop r0
    ret
    
radarInterruptHandler:
    mov r0, radarData           ; array start
    xor r1, r1                  ; i
    mov r7, RADAR_INVALID * 2   ; closest i
    mov r8, RADAR_INVALID       ; closest dist

    .loop:
        mov r4, byte [r0 + r1]
        add r1, 2
        cmp r4, [targetType]
        jne .skip               ; not a ship
        dec r1
        mov r4, byte [r0 + r1]
        inc r1
        cmp r4, r8
        jae .skip               ; farther than we have
        mov r7, r1              ; save i
        mov r8, r4              ; save dist
    .skip:
        cmp r1, RADAR_RAYCOUNT * 2
        jb .loop

    div r7, 2
    mov [targetDir], r7
    iret
    
; interrupt table
interruptTable:
    dd 0                       ; 0
    dd 0                       ; 1
    dd 0                       ; 2
    dd 0                       ; 3
    dd 0                       ; 4
    dd 0                       ; 5
    dd 0                       ; 6
    dd 0                       ; 7
    dd 0                       ; 8
    dd 0                       ; 9
    dd 0                       ; 10
    dd radarInterruptHandler   ; 11
    dd 0                       ; 12
    dd 0                       ; 13
    dd 0                       ; 14
    dd 0                       ; 15
    
; space for the radar data
radarData:
    rw RADAR_RAYCOUNT

;while (true)
;{
;   if (targetDir == RADAR_INVALID) {
;       setThrustSpeed(0);
;       setTurnSpeed(0);
;       setShooting(false);
;       continue;
;   }
;   
;   var heading = getHeading();
;   if (heading != targetDir) {
;       var direction = closestDirection(heading, targetDir);
;       setTurnSpeed(turnSpeed * direction);
;   } else {
;       setTurnSpeed(0);
;   }
;
;   var targetDiff = abs(heading - targetDir);
;   heading *= 2;
;   var headDist = radarData[heading + 1];
;
;   if (targetDiff >= 10) {
;       setThrustSpeed(0);
;   } else if (headDist <= reverseDist) {
;       setThrustSpeed(-thrustSpeed);
;   } else {
;       setThrustSpeed(thrustSpeed);
;   }
;
;   var headType = radarData[heading + 0];
;   if (headType == targetType) {
;       setShooting(true);
;   } else {
;       setShooting(false);
;   }
;}
