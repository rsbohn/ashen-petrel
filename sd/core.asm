ORG 20000

START:
    HALT
; use 'rho' for the return address
; stack diagrams include the return address
; TOS on the right
ROT:                ; (a b c rho -- b c a rho)
    STOR DB+400
    STOR DB+401
    XCH
    LOAD DB+401
    XCH
    LOAD DB+400
    SXIT 0

OVER:               ; (a b rho -- a b a rho)
    STOR DB+402
    XCH             ; (b a)
    DUP             ; (b a a)
    STOR DB+403     ; (b a)
    XCH             ; (a b)
    LOAD DB+403     ; (a b a)
    LOAD DB+402     ; (a b a rho)
    SXIT 0

DOT:                ; (a rho -- rho)
    STOR DB+404     ; save rho
    STOR DB+405     ; save value
    LOAD DB+405
    LSR #15
    LDI 7
    AND
    LDI 60          ; '0'
    ADD
    WIO 0
    LOAD DB+405
    LSR #12
    LDI 7
    AND
    LDI 60
    ADD
    WIO 0
    LOAD DB+405
    LSR #9
    LDI 7
    AND
    LDI 60
    ADD
    WIO 0
    LOAD DB+405
    LSR #6
    LDI 7
    AND
    LDI 60
    ADD
    WIO 0
    LOAD DB+405
    LSR #3
    LDI 7
    AND
    LDI 60
    ADD
    WIO 0
    LOAD DB+405
    LSR #0
    LDI 7
    AND
    LDI 60
    ADD
    WIO 0
    LOAD DB+404
    SXIT 0

ORG 21000
    DW ROT
TSTROT:
    LDI 30
    LDI 31
    LDI 32
    LOAD .-4
    SCAL 000    ;given  (30 31 32)
    HALT 0      ;expect (31 32 30)

    DW OVER
TSTOVER:
    LDI 10
    LDI 11
    LOAD .-3
    SCAL 000    ;given  (10 11)
    HALT 0      ;expect (10 11 10)

; should be AORG...
ORG 77770       ; SCAL N (1=77777, 2=77776)
    DW 0
    DW 0
    DW 0
    DW 0

    DW 0
    DW DOT
    DW OVER
    DW ROT