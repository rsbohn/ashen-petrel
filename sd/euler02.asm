;; Euler problem 2:
;; sum(even(fib)) where fib < #4_000_000
;; CONST limit #4_000_000
;; DOUBLE prev = 1
;; DOUBLE this = 2
;; DOUBLE next = 0
    ORG 200
INIT:
    LDI 1
    STOR UB
    LDI 2
    STOR TB
    LOAD NLFF
    WIO 2
    BR LOOP
LIMA:   DW 004400       ; low word (octal) for 4,000,000 decimal
LIMB:   DW 000075       ; high word
SUMA:   DW 000002
SUMB:   DW 000000
UA:     DW 000000
UB:     DW 000001     ; prev 01 (low word)
TA:     DW 000000
TB:     DW 000002     ; this 02 (low word)
LOOP:
    LOAD LIMB
    LOAD LIMA
    LOAD TA
    LOAD TB
    DCMP
    BL DONE
    LOAD TA
    LOAD TB
    LOAD UA
    LOAD UB
    DADD            ; add T and U
    LOAD TA         ; T -> U
    LOAD TB
    STOR UB
    STOR UA
    DDUP            ; keep T
    DUP
    WIO 1
    STOR TB         ; next -> T
    DUP
    WIO 1
    STOR TA
    ; add evens
    DUP             ; keep T.low
    BRO CHILL       ; skip if odd
    LOAD SUMB       ; sum if even
    LOAD SUMA
    DADD
    STOR SUMA
    STOR SUMB
    BR LOOP
CHILL:
    DEL, DEL        ; drop T
    BR LOOP

NLFF:
    DW $0A0C
DONE:
    LOAD NLFF       ; print NL FF then final answer
    WIO 2
    LOAD SUMA
    WIO 1
    LOAD SUMB
    WIO 1
    HALT
