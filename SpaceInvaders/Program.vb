Module Module1

    Const NO_KEY = 0
    Const CURSOR_LEFT = 1
    Const CURSOR_RIGHT = 2
    Const UNKNOWN_KEY = 99

    Const SPALTE_MAX = 79
    Const ZEILE_MAX = 24
    Const A_MIN = 1
    Const A_MAX_START = 2
    Const G_MIN = 1
    Const G_MAX = 9
    Const P_MIN = 0
    Const P_MAX = SPALTE_MAX

    Const BEWEGUNG_SPIELFIGUR = 10

    Function Tastatur_Abfrage() As Integer
        Dim cki As New ConsoleKeyInfo()
        If Console.KeyAvailable = False Then
            Return NO_KEY
        Else
            cki = Console.ReadKey(True)
            If cki.Key = ConsoleKey.LeftArrow Then
                Return CURSOR_LEFT
            ElseIf cki.Key = ConsoleKey.RightArrow Then
                Return CURSOR_RIGHT
            Else
                Return UNKNOWN_KEY
            End If
        End If
    End Function
    Sub Erzeuge_Zeile(ByRef Zeile() As Char, ByVal a_max As Integer)

        'Deklarieren der Variablen
        Dim a As Integer 'Anzahl der Hindernisblocks
        Dim x As Single
        Dim i, j As Integer
        Dim g As Integer 'Größe des Hindernisblocks
        Dim p As Integer 'Position des Hindernisblocks

        'Zeilenvektor mit Leerzeichen füllen
        For i = 0 To SPALTE_MAX
            Zeile(i) = " "
        Next

        'Anzahl A der HIndernisblocks zufällig ermitteln
        Randomize()
        x = VBMath.Rnd

        a = (a_max - A_MIN) * x + A_MIN
        'Console.WriteLine(A)

        'Für jeden der A Hindernisblocks:
        For i = 1 To a

            'Größe G des Hindernisblocks zufällig ermitteln
            Randomize()
            x = VBMath.Rnd

            g = (G_MAX - G_MIN) * x + G_MIN
            'Console.WriteLine("G: " & G)

            'Startposition P des Hindernisblocks zufällig ermitteln
            Randomize()
            x = VBMath.Rnd

            p = (P_MAX - P_MIN) * x + P_MIN
            'Console.WriteLine("P: " & P)

            'Für jedes der G Einzelhindernisse:
            For j = 1 To g

                'Prüfen ob Hinderniss innerhalb des Wertebereichs ist
                If p + j - 1 <= SPALTE_MAX Then

                    'Hinderniss an Position P+j-1 in den Zeilenvektor eintragen
                    Zeile(p + j - 1) = "X"

                End If

            Next

        Next

        ''Ausgabe zum Test
        'For i = 0 To SPALTE_MAX
        '    Console.Write(Zeile(i))
        'Next
        'Console.WriteLine()


    End Sub

    Sub Game_Over()
        Console.BackgroundColor = ConsoleColor.Red
        Console.ForegroundColor = ConsoleColor.White

        Console.Clear()

        Console.SetCursorPosition(0, 10)
        Console.WriteLine("  ___                        ___                      ")
        Console.WriteLine(" /  __/__    __   __   \__  \_  _ ____  ")
        Console.WriteLine("/   \  _\_  \  /     \/ _ \   /   |   \  \/ // _ \_  __ \ ")
        Console.WriteLine("\    \\  \/ _ \|  Y Y  \  _/  /    |    \   /\  _/|  | \/ ")
        Console.WriteLine(" \__  (__  /_||  /\_  > \___  /\/  \_  >_|    ")

        Console.ReadLine()
    End Sub


    Sub Spielablauf()
        Dim leben As Integer
        Dim spielfeld(ZEILE_MAX, SPALTE_MAX) As Char
        Dim zeile(SPALTE_MAX) As Char
        Dim z As Integer
        Dim s As Integer
        Dim taste As Integer
        Dim spielfigur_spalte As Integer
        Dim i As Integer
        Dim wartezeit As Single
        Dim a_max As Single

        'Startwerte setzen:
        leben = 5
        spielfigur_spalte = SPALTE_MAX / 2
        wartezeit = 200
        a_max = A_MAX_START

        'Hauptspielschleife des Spiels:
        Do
            'Neue Zeile erzeugen:
            Erzeuge_Zeile(zeile, a_max)

            'Alle Zeilen des Spielfelds um eine Zeile nach unten verschieben:
            'Rückwärtsschleife über die Zeilen:
            For z = ZEILE_MAX To 1 Step -1
                'Vorwärtsschleife über die Spalten:
                For s = 0 To SPALTE_MAX
                    'Ein Zeichen nach unten kopieren:
                    spielfeld(z, s) = spielfeld(z - 1, s)
                Next
            Next

            'Neue Zeile in die erste Zeile des Spielfelds eintragen:
            For s = 0 To SPALTE_MAX
                spielfeld(0, s) = zeile(s)
            Next

            'Spielfeld auf der Konsole ausgeben:
            Console.SetCursorPosition(0, 0)
            For z = 0 To ZEILE_MAX - 2
                For s = 0 To SPALTE_MAX
                    Console.Write(spielfeld(z, s))
                Next
                Console.WriteLine()
            Next

            'Zählschleife für schnelle Bewegung:
            For i = 1 To BEWEGUNG_SPIELFIGUR

                'Tastatur abfragen:
                taste = Tastatur_Abfrage()

                'alte Spielfigur löschen:
                Console.SetCursorPosition(spielfigur_spalte, ZEILE_MAX - 1)
                Console.Write(" ")

                'Position der Spielfigur berechnen:
                If taste = CURSOR_LEFT Then
                    spielfigur_spalte = spielfigur_spalte - 1
                    If spielfigur_spalte < 0 Then spielfigur_spalte = 0
                End If

                If taste = CURSOR_RIGHT Then
                    spielfigur_spalte = spielfigur_spalte + 1
                    If spielfigur_spalte > SPALTE_MAX Then spielfigur_spalte = SPALTE_MAX
                End If

                'Spielfigur auf der Konsole ausgeben:
                Console.SetCursorPosition(spielfigur_spalte, ZEILE_MAX - 1)
                Console.Write("#")

                'Kollisionsprüfung:
                If spielfeld(22, spielfigur_spalte) = "X" Then
                    'Kollision erkannt:
                    leben = leben - 1
                    Console.Beep()

                    'Hindernis löschen:
                    spielfeld(22, spielfigur_spalte) = " "
                End If

                'Anzeige der Leben-Anzahl:
                Console.SetCursorPosition(0, ZEILE_MAX)
                Console.Write("Leben: " & leben & " ")

                'Warten:
                Threading.Thread.Sleep(wartezeit / BEWEGUNG_SPIELFIGUR)
            Next

            'Tastaturpuffer leeren
            Do
                taste = Tastatur_Abfrage()
            Loop Until taste = NO_KEY

            'Wartezeit verringern:
            wartezeit = wartezeit * 0.99
            If wartezeit < 0 Then wartezeit = 0
            'Console.SetCursorPosition(15, ZEILE_MAX)
            'Console.WriteLine(wartezeit)

            'Hindernisdichte erhöhen:
            a_max = a_max * 1.03
            ' Console.SetCursorPosition(15, ZEILE_MAX)
            'Console.WriteLine(a_max)



        Loop Until leben <= 0

        Game_Over()


    End Sub

    Sub Main()

        Console.CursorVisible = False

        Spielablauf()
    End Sub


End Module