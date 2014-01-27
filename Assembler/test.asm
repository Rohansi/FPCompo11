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

varTargetType:
    db VAR_RADARVALUE
    dd targetType

varTargetDir:
    db VAR_GENERAL
    dd targetDir

;
; Variable declarations
;

turnSpeed:
    dd 75

thrustSpeed:
    dd 100

reverseDist:
    dd 15

targetType:
    dd RADAR_PLAYER

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
    mul r0, 2
    mov r1, byte [r0 + radarData] ; type
    cmp r1, [targetType]
    jne .notFacingTarget
    xor r7, r7
    mov r8, [thrustSpeed]
    int DEV_ENGINES
    inc r7
    int DEV_GUNS
    jmp main
.notFacingTarget:
    xor r7, r7
    int DEV_GUNS
    mov r1, byte [r0 + radarData + 1] ; dist
    cmp r1, [reverseDist]
    ja main
    xor r8, r8
    sub r8, [thrustSpeed]
    int DEV_ENGINES
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

; this is quite outdated but still what it does, maybe.

;while (true)
;{
;   if (target == 127) {
;       setThrustSpeed(0);
;       setTurnSpeed(0);
;       setShooting(false);
;       continue;
;   }
;   
;   var heading = getHeading();
;   if (heading != target) {
;       var direction = closestDirection();
;       setTurnSpeed(direction * 75);
;   } else {
;       setTurnSpeed(0);
;   }
;   
;   var headingRay = radar[heading];
;   if ((headingRay >> 8) == 1) {
;       setShooting(true);
;       setThrustSpeed(-100);
;   } else {
;       setShooting(false);
;       if ((headingRay & 0xFF) <= 15) {
;           setThrustSpeed(-100);
;       }
;   }
;}
