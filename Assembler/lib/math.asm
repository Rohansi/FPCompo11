
; int sin(int x)
sin:
    push bp
    mov bp, sp
    push r1 ; x
    push r2 ; i
    push r3 ; v0
    push r4 ; v1
    push r5 ; t

    mov r1, [bp + 8]
    add r1, (RADAR_RAYCOUNT / 4) * 1
    jmp sincosImpl

; int cos(int x)
cos:
    push bp
    mov bp, sp
    push r1 ; x
    push r2 ; i
    push r3 ; v0
    push r4 ; v1
    push r5 ; t

    mov r1, [bp + 8]
    add r1, (RADAR_RAYCOUNT / 4) * 2
    ;jmp sincosImpl

sincosImpl:
    rem r1, RADAR_RAYCOUNT
    cmp r1, 0
    jae @f
    add r1, RADAR_RAYCOUNT
@@:

    mov r2, r1
    div r2, 8
    mov r3, byte [r2 + sincosLookupTable + 0]
    mov r4, byte [r2 + sincosLookupTable + 1]
    mov r5, r2
    inc r5
    mul r5, 8
    sub r5, r1
    mul r5, 100
    div r5, 8

    mov r0, 100
    sub r0, r5
    mul r0, r4
    mul r3, r5
    add r0, r3
    div r0, 100

.return:
    pop r5
    pop r4
    pop r3
    pop r2
    pop r1
    pop bp
    retn 4

sincosLookupTable:
    db   0,  38,  71,  93,  99,  91,  68,  34
    db -04, -43, -74, -94, -99, -88, -64, -29, 7
