;; Euler Problem 4
;; Find the largest palindrome made from the product of two 3-digit numbers.
;;
;; Algorithm:
;;   For I = 999 downto 100:
;;     For J = I downto 100:
;;       P = I * J  (32-bit via LMPY)
;;       If P <= BEST: break inner loop (J decreasing, no improvement possible)
;;       If palindrome(P): BEST = P
;;   Print BEST
;;
;; Digit extraction: divide P by 10 five times using LDIV to get D6..D2,
;; then D1 = remaining quotient. Palindrome iff D1==D6, D2==D5, D3==D4.
;;
;; LDIV (C,B)/A -> TOS=remainder, second=quotient
;; LMPY b*a    -> TOS=low, second=high
;; DCMP (D,C) vs (B,A): push high first, low second (TOS=low)

    ORG 200
START:
    ZERO
    STOR BESTH
    STOR BESTL
    LOAD C999
    STOR I

OUTER:
    LOAD I
    LDI 144             ;; 100 decimal = 144 octal
    SUB, DEL
    BGE .+2             ;; I >= 100: skip BR DONE
    BR DONE
    LOAD I
    STOR J

INNER:
    LOAD J
    LDI 144
    SUB, DEL
    BGE .+2             ;; J >= 100: skip BR NEXT_I
    BR NEXT_I

    ;; P = I * J (32-bit product)
    LOAD I
    LOAD J
    LMPY                ;; TOS=P_L (low), B=P_H (high)
    STOR PTMPL
    STOR PTMPH

    ;; Compare P vs BEST; if P <= BEST break inner loop
    LOAD PTMPH
    LOAD PTMPL
    LOAD BESTH
    LOAD BESTL
    DCMP                ;; (D=P_H, C=P_L) vs (B=BEST_H, A=BEST_L)
    BG PALCHK           ;; P > BEST: check palindrome
    BR NEXT_I           ;; P <= BEST: next I

PALCHK:
    ;; Copy P into work variable
    LOAD PTMPH
    STOR WH
    LOAD PTMPL
    STOR WL

    ;; Extract D6 (units digit): WH:WL / 10
    ZERO
    LOAD WH
    LDI 12              ;; 10 decimal
    LDIV                ;; A=REM1 (TOS), B=QH
    XCH                 ;; A=QH (TOS), B=REM1
    STOR QH             ;; save QH; stack: (REM1)
    LOAD WL
    LDI 12
    LDIV                ;; A=D6 (TOS), B=QL
    XCH                 ;; A=QL (TOS), B=D6
    STOR WL             ;; save QL as new WL
    STOR D6             ;; save digit
    LOAD QH
    STOR WH

    ;; Extract D5 (tens digit)
    ZERO
    LOAD WH
    LDI 12
    LDIV
    XCH
    STOR QH
    LOAD WL
    LDI 12
    LDIV
    XCH
    STOR WL
    STOR D5
    LOAD QH
    STOR WH

    ;; Extract D4 (hundreds digit)
    ZERO
    LOAD WH
    LDI 12
    LDIV
    XCH
    STOR QH
    LOAD WL
    LDI 12
    LDIV
    XCH
    STOR WL
    STOR D4
    LOAD QH
    STOR WH

    ;; Extract D3 (thousands digit)
    ZERO
    LOAD WH
    LDI 12
    LDIV
    XCH
    STOR QH
    LOAD WL
    LDI 12
    LDIV
    XCH
    STOR WL
    STOR D3
    LOAD QH
    STOR WH

    ;; Extract D2 (ten-thousands digit)
    ZERO
    LOAD WH
    LDI 12
    LDIV
    XCH
    STOR QH
    LOAD WL
    LDI 12
    LDIV
    XCH
    STOR WL
    STOR D2
    LOAD QH
    STOR WH

    ;; D1 = remaining quotient (hundred-thousands digit)
    LOAD WL
    STOR D1

    ;; Palindrome check: D1==D6, D2==D5, D3==D4
    LOAD D1
    LOAD D6
    SUB, DEL
    BNE NEXT_J

    LOAD D2
    LOAD D5
    SUB, DEL
    BNE NEXT_J

    LOAD D3
    LOAD D4
    SUB, DEL
    BNE NEXT_J

    ;; Palindrome found and P > BEST; update BEST
    LOAD PTMPH
    STOR BESTH
    LOAD PTMPL
    STOR BESTL

NEXT_J:
    LOAD J
    DECA
    STOR J
    BR INNER

NEXT_I:
    LOAD I
    DECA
    STOR I
    BR OUTER

DONE:
    ;; Print result (set lptradix A for decimal before running)
    LOAD BESTH
    WIO 1               ;; high word
    LOAD BESTL
    WIO 1               ;; low word
    HALT

;; Constants
C999:   DW 01747        ;; 999 decimal

;; Variables
I:      DW 0
J:      DW 0
BESTH:  DW 0
BESTL:  DW 0
PTMPH:  DW 0
PTMPL:  DW 0
WH:     DW 0
WL:     DW 0
QH:     DW 0
D1:     DW 0
D2:     DW 0
D3:     DW 0
D4:     DW 0
D5:     DW 0
D6:     DW 0
