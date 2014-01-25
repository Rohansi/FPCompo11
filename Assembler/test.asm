include 'loonyvm.inc'

DEV_NAVIGATION  = 10
DEV_RADAR       = 11
DEV_ENGINES     = 12
DEV_GUNS        = 13
DEV_DEBUG       = 14

; enables interrupts
ivt interruptTable
sti

; enable radar
mov r7, radarData
int DEV_RADAR

;while (true)
;{
;   if (target == 255) {
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
;   if ((headingRay >> 8) == 2) {
;       setShooting(true);
;       setThrustSpeed(-100);
;   } else {
;       setShooting(false);
;       if ((headingRay & 0xFF) <= 15) {
;           setThrustSpeed(-100);
;       }
;   }
;}

main:
    cmp [target], 127
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

    cmp r0, [target]
    je .facingTarget
    mov r1, [target]
    call closestDirection
    mov r8, 75
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
    cmp r1, 1
    jne .notFacingTarget
    xor r7, r7
    mov r8, 100
    int DEV_ENGINES
    inc r7
    int DEV_GUNS
    jmp main
.notFacingTarget:
    xor r7, r7
    int DEV_GUNS
    mov r1, byte [r0 + radarData + 1] ; dist
    cmp r1, 15
    ja main
    mov r8, -100
    int DEV_ENGINES
    jmp main
    
; blank interrupt handler
defaultInterruptHandler:
    iret
 
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
    cmp r0, 126 / 2
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
    
target:
    dd 127
    
radarInterruptHandler:
    mov r0, radarData  ; array start
    xor r1, r1         ; i
    mov r7, 127*2      ; closest i
    mov r8, 127        ; closest dist

    .loop:
        mov r4, byte [r0 + r1]
        add r1, 2
        cmp r4, 1
        jne .skip      ; not a ship
        dec r1
        mov r4, byte [r0 + r1]
        inc r1
        cmp r4, r8
        jae .skip      ; farther than we have
        mov r7, r1     ; save i
        mov r8, r4     ; save dist
    .skip:
        cmp r1, 126 * 2
        jb .loop

    div r7, 2
    mov [target], r7
    iret
    
; interrupt table
interruptTable:
    dd defaultInterruptHandler ; 0
    dd defaultInterruptHandler ; 1
    dd defaultInterruptHandler ; 2
    dd defaultInterruptHandler ; 3
    dd defaultInterruptHandler ; 4
    dd defaultInterruptHandler ; 5
    dd defaultInterruptHandler ; 6
    dd defaultInterruptHandler ; 7
    dd defaultInterruptHandler ; 8
    dd defaultInterruptHandler ; 9
    dd defaultInterruptHandler ; 10
    dd radarInterruptHandler   ; 11
    dd defaultInterruptHandler ; 12
    dd defaultInterruptHandler ; 13
    dd defaultInterruptHandler ; 14
    dd defaultInterruptHandler ; 15
    
; space for the radar data
radarData:
    rw 126
