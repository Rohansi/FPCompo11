include 'glitch.inc'

invoke entryPoint
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
    push bp
    mov bp, sp

    ; enables interrupts
    ivt interruptTable
    sti

    ; enable radar
    mov r7, radarData
    int DEV_RADAR

    .while 1
        .if [targetDir] <> RADAR_INVALID
            mov r6, [targetDir]
        .else
            xor r7, r7
            xor r8, r8
            int DEV_ENGINES
            int DEV_GUNS
            inc r7
            int DEV_ENGINES
            .continuew
        .endif

        mov r7, 2
        int DEV_NAVIGATION
        mov r0, r7
        push r0     ; r0 = heading

        .if r0 <> r6
            invoke closestDirection, r0, r6
            mov r7, 1
            mov r8, [turnSpeed]
            mul r8, r0
            int DEV_ENGINES
        .else
            mov r7, 1
            xor r8, r8
            int DEV_ENGINES
        .endif

        pop r0
        mov r5, r0
        sub r5, r6
        abs r5      ; r5 = targetDiff
        mul r0, 2

        .if r5 >= [noThrustDiff]
            xor r7, r7
            xor r8, r8
            int DEV_ENGINES
        .elseif byte [r0 + radarData + 1] <= [reverseDist]
            xor r7, r7
            mov r8, [thrustSpeed]
            neg r8
            int DEV_ENGINES
        .else
            xor r7, r7
            mov r8, [thrustSpeed]
            int DEV_ENGINES
        .endif

        .if byte [r0 + radarData + 0] = [targetType]
            mov r7, 1
            int DEV_GUNS
        .else
            xor r7, r7
            int DEV_GUNS
        .endif
    .endw
.return:
    pop bp
    ret

; Returns the direction to spin to reach a target angle the fastest
; int closestDirection(int heading, int target)
closestDirection:
    push bp
    mov bp, sp
    push r1
    push r2
    push r3

    mov r0, 1
    mov r1, [bp + 8]    ; heading
    xor r2, r2          ; temp1
    xor r3, r3          ; temp2
    sub r1, [bp + 12]
    cmp r1, 0
    jbe .below0
    inc r2
.below0:
    abs r1
    cmp r1, RADAR_RAYCOUNT / 2
    jbe .belowHalf
    inc r3
.belowHalf:
    xor r2, r3
    jz .return
    mov r0, -1

.return:
    pop r3
    pop r2
    pop r1
    pop bp
    retn 8

radarInterruptHandler:
    mov r0, radarData           ; ptr to ray
    xor r1, r1                  ; ray number
    mov r4, RADAR_INVALID       ; target dir
    mov r5, RADAR_INVALID       ; target dist

    .loop:
        cmp byte [r0], [targetType]
        jne .notTarget
        cmp byte [r0 + 1], r5
        jae .continue           ; farther than we have
        mov r4, r1
        mov r5, byte [r0 + 1]
    .notTarget:
    .continue:
        add r0, 2
        inc r1
        cmp r1, RADAR_RAYCOUNT
        jb .loop

    mov [targetDir], r4
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
