include 'glitch.inc'

jmp entryPoint
rb 8 - ($-$$)

;
; Variable locations to corrupt
;

variableCount:
    dd 6

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

varFriendType:
    db VAR_RADARVALUE
    dd friendType

;
; Variable declarations
;

turnSpeed:
    dd 85

thrustSpeed:
    dd 75

reverseDist:
    dd 15

noThrustDiff:
    dd 15

targetType:
    dd RADAR_ENEMY

targetDir:
    dd RADAR_INVALID

friendType:
    dd RADAR_ALLY

friendDir:
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
    je .noTarget
    mov r6, [targetDir]
    jmp .logic
.noTarget:
    cmp [friendDir], RADAR_INVALID
    je .noFriend
    mov r6, [friendDir]
    jmp .logic
.noFriend:
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
    cmp r0, r6
    je .facingTarget
    mov r1, r6
    call closestDirection
    mov r8, r6
    mul r8, r2
    jmp .setTurn
.facingTarget:
    xor r8, r8
.setTurn:
    mov r7, 1
    int DEV_ENGINES

    pop r0
    mov r5, r0
    sub r5, r6
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
    mov r0, radarData           ; ptr to type
    mov r1, r0 + 1              ; ptr to dist
    xor r2, r2                  ; ray number
    mov r4, RADAR_INVALID       ; friend dir
    mov r5, RADAR_INVALID       ; friend dist
    mov r6, RADAR_INVALID       ; target dir
    mov r7, RADAR_INVALID       ; target dist

    .loop:
        cmp byte [r0], [friendType]
        jne .notFriend
        cmp byte [r1], r5
        jae .continue           ; farther than we have
        mov r4, r2
        mov r5, byte [r1]
        jmp .continue
    .notFriend:
        cmp byte [r0], [targetType]
        jne .notTarget
        cmp byte [r1], r5           
        jae .continue           ; farther than we have
        mov r6, r2
        mov r7, byte [r1]
        jmp .continue
    .notTarget:
    .continue:
        add r0, 2
        add r1, 2
        inc r2
        cmp r2, RADAR_RAYCOUNT
        jb .loop

    mov [friendDir], r4
    mov [targetDir], r6
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
;   int moveDir;
;
;   if (targetDir != RADAR_INVALID) {
;       moveDir = targetDir;
;   } else if (friendDir != RADAR_INVALID) {
;       moveDir = friendDir;
;   } else {
;       setThrustSpeed(0);
;       setTurnSpeed(0);
;       setShooting(false);
;       continue;
;   }
;
;   int heading = getHeading();
;   if (heading != moveDir) {
;       int direction = closestDirection(heading, moveDir);
;       setTurnSpeed(turnSpeed * direction);
;   } else {
;       setTurnSpeed(0);
;   }
;
;   int targetDiff = abs(heading - moveDir);
;   heading *= 2;
;   int headDist = radarData[heading + 1];
;
;   if (targetDiff >= 10) {
;       setThrustSpeed(0);
;   } else if (headDist <= reverseDist) {
;       setThrustSpeed(-thrustSpeed);
;   } else {
;       setThrustSpeed(thrustSpeed);
;   }
;
;   int headType = radarData[heading + 0];
;   if (headType == targetType) {
;       setShooting(true);
;   } else {
;       setShooting(false);
;   }
;}
