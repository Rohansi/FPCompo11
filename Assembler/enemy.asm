include 'glitch.inc'

invoke entryPoint
rb 8 - ($-$$)

;
; Corruptable variables
;

corruptableCount:
    dd 5

turnSpeed:
    dd 65
    db VAR_SPEED

thrustSpeed:
    dd 75
    db VAR_SPEED

reverseDist:
    dd 250
    db VAR_GENERAL

noThrustDiff:
    dd 15
    db VAR_GENERAL

targetType:
    dd RADAR_ENEMY
    db VAR_RADARVALUE

;
; Additional variables
;

targetDir:
    dd RADAR_INVALIDRAY

helpResponding:
    dd 0
helpX:
    dd 0
helpY:
    dd 0

;
; Ship code
;
    cmpxchg word [r0 + 0xFFFFFFFF], word [r0 + 0xFFFFFFFF]
entryPoint:
    push bp
    mov bp, sp

    ; enables interrupts
    ivt interruptTable
    sti

    ; enable radar
    invoke setRadarPointer, radarData

    ; enable broadcast
    mov r0, 1
    mov r1, packetData
    int DEV_BROADCAST

    .while 1
        .if [targetDir] <> RADAR_INVALIDRAY
            mov r6, [targetDir]
            mov [helpResponding], 0
            invoke callForHelp
        .elseif [helpResponding] <> 0
            xor r0, r0
            int DEV_NAVIGATION
            push r0

            invoke getDistance, r0, r1, [helpX], [helpY]
            .if r0 <= 800
                mov [helpResponding], 0
                pop r0
                .continuew
            .endif

            pop r0
            invoke getDirection, r0, r1, [helpX], [helpY]
            mov r6, r0
        .else
            invoke setThrustSpeed, 0
            invoke setTurnSpeed, 0
            invoke setShooting, 0
            .continuew
        .endif

        invoke getHeading
        push r0     ; r0 = heading

        .if r0 <> r6
            invoke getTurnDirection, r0, r6
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
        mul r0, 3

        .if r5 >= [noThrustDiff]
            invoke setThrustSpeed, 0
        .elseif word [r0 + radarData + 1] <= [reverseDist]
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

callForHelp:
    push bp
    mov bp, sp
    push r0
    push r1
    push r2
    push r3
    push r4

    xor r0, r0
    int DEV_NAVIGATION
    mov r2, r1
    mov r1, r0

    mov r3, packetOutData

    mov r4, [targetDir]
    mul r4, 3
    add r4, radarData

    cmp byte [r4], [targetType]
    jne .return

    invoke sin, [targetDir]
    mul r0, word [r4 + 1]
    div r0, 100
    mov [r3 + 0], r1 + r0

    invoke cos, [targetDir]
    neg r0
    mul r0, word [r4 + 1]
    div r0, 100
    mov [r3 + 4], r2 + r0

    mov r0, 2
    mov r1, packetOutData
    int DEV_BROADCAST

.return:
    pop r4
    pop r3
    pop r2
    pop r1
    pop r0
    pop bp
    ret

radarIrqHandler:
    mov r0, radarData           ; ptr to ray
    xor r1, r1                  ; ray number
    mov r4, RADAR_INVALIDTYPE   ; target dir
    mov r5, RADAR_INVALIDDIST   ; target dist

    .loop:
        cmp byte [r0], [targetType]
        jne .notTarget
        cmp word [r0 + 1], r5
        jae .continue           ; farther than we have
        mov r4, r1
        mov r5, word [r0 + 1]
    .notTarget:
    .continue:
        add r0, 3
        inc r1
        cmp r1, RADAR_RAYCOUNT
        jb .loop

    mov [targetDir], r4
    iret

broadcastIrqHandler:
    mov r6, packetData

    .if [helpResponding] <> 0
        xor r0, r0
        int DEV_NAVIGATION
        mov r2, r1
        mov r1, r0

        invoke getDistance, r1, r2, [helpX], [helpY]
        mov r3, r0

        invoke getDistance, r1, r2, [r6 + 0], [r6 + 4]

        .if r0 >= r3 
            iret
        .endif
    .endif

    mov [helpResponding], 1
    mov [helpX], [r6 + 0]
    mov [helpY], [r6 + 4]
    iret

include 'lib/navigation.asm'
include 'lib/radar.asm'
include 'lib/engines.asm'
include 'lib/guns.asm'
include 'lib/direction.asm'
include 'lib/math.asm'

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
    dd radarIrqHandler         ; 11
    dd 0                       ; 12
    dd 0                       ; 13
    dd broadcastIrqHandler     ; 14
    dd 0                       ; 15

; space for the radar data
radarData:
    rb 3 * RADAR_RAYCOUNT

packetData:
    rb 32

packetOutData:
    rb 32

db 255

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
;       var direction = getTurnDirection(heading, targetDir);
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
