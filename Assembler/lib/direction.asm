
; Returns the optimal spin direction from heading to target
; int getSpinDirection(int heading, int target)
getSpinDirection:
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
    cmp r1, (RADAR_RAYCOUNT / 2)
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

; Return approximate direction from coord1 to coord2
; int getDirection(int x1, int y1, int x2, int y2)
getDirection:
    push bp
    mov sp, bp
    push r1 ; xdiff
    push r2 ; ydiff
    push r3 ; diff1
    push r4 ; diff2

    mov r1, [bp + 16]
    sub r1, [bp + 8]
    mov r2, [bp + 12]
    sub r2, [bp + 20]
    mov r3, r2
    sub r3, r1
    mov r4, [bp + 20]
    sub r4, [bp + 12]
    sub r4, r1

.southDir = RADAR_RAYCOUNT / 4
.northDir = RADAR_RAYCOUNT - .southDir
.dirDiff  = RADAR_RAYCOUNT / 8

    cmp r2, 0
    jae .north
.south:
        mov r0, .southDir
        cmp r4, 0
        jbe .west
        sub r0, .dirDiff
        jmp .return
    .west:
        cmp r3, 0
        jae .return
        add r0, .dirDiff
        jmp .return
.north:
        mov r0, .northDir
        cmp r3, 0
        jbe .east
        add r0, .dirDiff
        jmp .return
    .east:
        cmp r4, 0
        jae .return
        sub r0, .dirDiff

.return:
    pop r4
    pop r3
    pop r2
    pop r1
    pop bp
    retn 16

; int spinDirection(int heading, int target) {
;     int result = 1
;     int diff = heading - target;
;     if (diff > 0 ^ abs(diff) > 63)
;         result = -1;
;     return result;
; }

; int direction(int x1, int y1, int x2, int y2) {
;     int xdiff = x2 - x1;
;     int ydiff = y1 - y2;
;     int diff1 = xdiff - ydiff;
;     int diff2 = xdiff - (y2 - y1);
;     int result;
;     
;     if (ydiff < 0) {
;         result = 32;
;         if (diff2 > 0)
;             dir -= 16;
;         else
;             dir += 16;
;     } else {
;         result = 96;
;         if (diff1 > 0)
;             dir += 16;
;         else
;             dir -= 16;
;     }
;     
;     return result;
; }
