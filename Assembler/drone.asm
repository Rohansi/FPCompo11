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
    invoke setRadarPointer, radarData

    .while 1
        .if [targetDir] <> RADAR_INVALID
            mov r6, [targetDir]
        .elseif [friendDir] <> RADAR_INVALID
            mov r6, [friendDir]
            mul r6, 2

            ; stop moving if near a friend
            .if byte [r6 + radarData + 1] <= [reverseDist]
                invoke setThrustSpeed, 0
                invoke setTurnSpeed, 0
                invoke setShooting, 0
                .continuew
            .endif

            mov r6, [friendDir]
        .else
            invoke setThrustSpeed, 0
            invoke setTurnSpeed, 0
            invoke setShooting, 0
            .continuew
        .endif      ; r6 = moveDir

        invoke getHeading
        push r0     ; r0 = heading

        .if r0 <> r6
            invoke getSpinDirection, r0, r6
            mov r1, [turnSpeed]
            mul r1, r0
            invoke setTurnSpeed, r1
        .else
            invoke setTurnSpeed, 0
        .endif

        pop r0
        mov r5, r0
        sub r5, r6
        abs r5      ; r5 = targetDiff
        mul r0, 2

        .if r5 >= [noThrustDiff]
            invoke setThrustSpeed, 0
        .elseif byte [r0 + radarData + 1] <= [reverseDist]
            mov r1, [thrustSpeed]
            neg r1
            invoke setThrustSpeed, r1
        .else
            invoke setThrustSpeed, [thrustSpeed]
        .endif

        .if byte [r0 + radarData + 0] = [targetType]
            invoke setShooting, 1
        .else
            invoke setShooting, 0
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

include 'lib/navigation.asm'
include 'lib/radar.asm'
include 'lib/engines.asm'
include 'lib/guns.asm'
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
;       int direction = getSpinDirection(heading, moveDir);
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
