include 'glitch.inc'

invoke entryPoint
rb 8 - ($-$$)

;
; Corruptable variables
;
corruptableVariables:
    corruptableCount:
        dd 6

    turnSpeed:
        dd 85
        db VAR_SPEED

    thrustSpeed:
        dd 75
        db VAR_SPEED

    reverseDist:
        dd 20
        db VAR_GENERAL

    noThrustDiff:
        dd 15
        db VAR_GENERAL

    targetType:
        dd RADAR_ENEMY
        db VAR_RADARVALUE

    friendType:
        dd RADAR_ALLY
        db VAR_RADARVALUE

;
; Additional variables
;

targetDir:
    dd RADAR_INVALID

friendDir:
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
        .elseif [friendDir] <> RADAR_INVALID
            mov r6, [friendDir]
            mul r6, 2

            ; stop moving if near a friend
            .if byte [r6 + radarData + 1] <= [reverseDist]
                xor r7, r7
                xor r8, r8
                int DEV_ENGINES
                int DEV_GUNS
                inc r7
                int DEV_ENGINES
                .continuew
            .endif

            mov r6, [friendDir]
        .else
            xor r7, r7
            xor r8, r8
            int DEV_ENGINES
            int DEV_GUNS
            inc r7
            int DEV_ENGINES
            .continuew
        .endif      ; r6 = moveDir

        mov r7, 2
        int DEV_NAVIGATION
        mov r0, r7
        push r0     ; r0 = heading

        .if r0 <> r6
            invoke spinDirection, r0, r6
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
    
radarInterruptHandler:
    mov r0, radarData           ; ptr to type
    xor r1, r1                  ; ray number
    mov r4, RADAR_INVALID       ; friend dir
    mov r5, RADAR_INVALID       ; friend dist
    mov r6, RADAR_INVALID       ; target dir
    mov r7, RADAR_INVALID       ; target dist

    .loop:
        cmp byte [r0], [friendType]
        jne .notFriend
        cmp byte [r0 + 1], r5
        jae .continue           ; farther than we have
        mov r4, r1
        mov r5, byte [r0 + 1]
        jmp .continue
    .notFriend:
        cmp byte [r0], [targetType]
        jne .notTarget
        cmp byte [r0 + 1], r7
        jae .continue           ; farther than we have
        mov r6, r1
        mov r7, byte [r0 + 1]
    .notTarget:
    .continue:
        add r0, 2
        inc r1
        cmp r1, RADAR_RAYCOUNT
        jb .loop

    mov [friendDir], r4
    mov [targetDir], r6
    iret

include 'lib/direction.asm'

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
;       int direction = spinDirection(heading, moveDir);
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
