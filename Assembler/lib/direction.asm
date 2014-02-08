
; Returns the optimal turn direction from heading to target
; int getTurnDirection(int heading, int target)
getTurnDirection:
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
    mov bp, sp
    push r1 ; xdiff
    push r2 ; ydiff
    push r3 ; diff1
    push r4 ; diff2

    mov r1, [bp + 16] 
    sub r1, [bp + 8]  ; xdiff = x2 - x1
    mov r2, [bp + 12]
    sub r2, [bp + 20] ; ydiff = y1 - y2
    mov r3, r1
    sub r3, r2        ; diff1 = xdiff - ydiff
    push r1
    mov r4, [bp + 20]
    sub r4, [bp + 12]
    sub r1, r4        
    mov r4, r1        ; diff2 = xdiff - (y2 - y1)
    pop r1

.southDir = RADAR_RAYCOUNT / 4
.northDir = RADAR_RAYCOUNT - .southDir
.dirDiff  = RADAR_RAYCOUNT / 8

    .if r2 < 0
        mov r0, .southDir

        .if r4 > 0
            sub r0, .dirDiff
        .elseif r3 < 0
            add r0, .dirDiff
        .endif
    .else
        mov r0, .northDir

        .if r3 > 0
            add r0, .dirDiff
        .elseif r4 < 0
            sub r0, .dirDiff
        .endif
    .endif

.return:
    pop r4
    pop r3
    pop r2
    pop r1
    pop bp
    retn 16

; Return approximate distance from coord1 to coord2
; int getDirection(int x1, int y1, int x2, int y2)
getDistance:
    push bp
    mov bp, sp
    push r1

    mov r0, [bp + 8]
    sub r0, [bp + 16]
    abs r0
    mov r1, [bp + 12]
    sub r1, [bp + 20]
    abs r1
    add r0, r1

.return:
    pop r1
    pop bp
    retn 16

; int getTurnDirection(int heading, int target) {
;     int result = 1
;     int diff = heading - target;
;     if (diff > 0 ^ abs(diff) > 63)
;         result = -1;
;     return result;
; }

; int getDirection(int x1, int y1, int x2, int y2) {
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

; int getDistance(int x1, int y1, int x2, int y2) {
;     return abs(x1 - x2) + abs(y1 - y2);
; }
