;; Find the largest prime factor of NU
;; For a solution see sd/pfind.asm
    ORG 200
START:
    LDI 0
    STAX, ZERO  ; R=0
LOOP:
    LOAD NU,X   ; (Ni R)
    LOAD F0     ; (d Ni R)
    LDIV        ; (R Qi)
    XCH         ; (Qi R)
    STOR NQ,X   ; (R)
    INCX, LDXA
    LDI 4
    SUB, DEL
    BL LOOP
    ; assert TOS (R) is zero
    HALT

NQ: 
    DQ 000000 000000 000000 000000
NU:
    DQ #600851475143
NV:
    DQ #8462696833
    ;; NU = 71 * NV
    ;; NU = 71 * 839 * 1471 * 6857
F0: DW #071
F1: DW #839
F2: DW #1471
F3: DW #6857
