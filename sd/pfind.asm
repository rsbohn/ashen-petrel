;; Prime factor finder for 64-bit N.
;; Uses 16-bit trial divisors and prints factors in decimal via WIO 1.
;; Set the line printer radix to decimal before running: lptradix A

    ORG 200
START:
    LDI 2
    STOR D

MAIN:
    ; Divide N by D -> Q (quotient), REM (remainder)
    ZERO
    STOR REM
    LDXI 0
DIV_LOOP:
    LOAD REM
    LOAD N,X
    LOAD D
    LDIV
    STOR REM
    STOR Q,X
    INCX
    LDXA
    LDI 4
    SUB
    DEL
    BL DIV_LOOP

    LOAD REM
    TEST
    DEL
    BNE .+2
    BR FACTOR

    ; If Q < D, we're done (N is prime or 1).
    LDXI 0
    LOAD Q,X
    TEST
    DEL
    BE .+2
    BR INC_D
    INCX
    LOAD Q,X
    TEST
    DEL
    BE .+2
    BR INC_D
    INCX
    LOAD Q,X
    TEST
    DEL
    BE .+2
    BR INC_D
    INCX
    LOAD Q,X
    LOAD D
    SUB
    DEL
    BGE .+2
    BR PRINT_N

INC_D:
    LOAD D
    LDI 2
    SUB
    DEL
    BNE .+2
    BR SET_D3
    LOAD D
    LDI 2
    ADD
    STOR D
    BR MAIN

SET_D3:
    LDI 3
    STOR D
    BR MAIN

FACTOR:
    LOAD D
    WIO 1
    ; N = Q
    LDXI 0
COPY_LOOP:
    LOAD Q,X
    STOR N,X
    INCX
    LDXA
    LDI 4
    SUB
    DEL
    BL COPY_LOOP

    ; If N == 1, done; otherwise keep dividing by same D.
    LDXI 0
    LOAD N,X
    TEST
    DEL
    BE .+2
    BR MAIN
    INCX
    LOAD N,X
    TEST
    DEL
    BE .+2
    BR MAIN
    INCX
    LOAD N,X
    TEST
    DEL
    BE .+2
    BR MAIN
    INCX
    LOAD N,X
    LDI 1
    SUB
    DEL
    BNE .+2
    BR DONE
    BR MAIN

PRINT_N:
    ; Print remaining factor if N != 1.
    LDXI 0
    LOAD N,X
    TEST
    DEL
    BE .+2
    BR DO_PRINT_N
    INCX
    LOAD N,X
    TEST
    DEL
    BE .+2
    BR DO_PRINT_N
    INCX
    LOAD N,X
    TEST
    DEL
    BE .+2
    BR DO_PRINT_N
    INCX
    LOAD N,X
    LDI 1
    SUB
    DEL
    BNE .+2
    BR DONE

DO_PRINT_N:
    LDXI 3
    LOAD N,X
    WIO 1

DONE:
    HALT

D:   DW 000002
REM: DW 000000
Q:   DQ 000000
N:   DQ #600851475143
