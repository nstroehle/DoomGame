Module Module1

    Const NO_KEY = 0
    Const CURSOR_LEFT = 1
    Const CURSOR_RIGHT = 2
    Const UNKNOWN_KEY = 99
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
    Sub ZeilenErzeugung(ByRef Zeile() As Char)

        'Deklarieren der Variablen
        Dim A As Integer    'Anzahl der Hindernisblocks
        Dim X As Single
        Dim i As Integer
        Dim G As Integer    'Größe des Hindernisblocks
        Dim P As Integer    'Position des Hindernisblocks


        'Zeilenvektor mit Leerzeichen füllen
        For i = 0 To 79
            Zeile(i) = " "
        Next

        'Anzahl A der HIndernisblocks zufällig ermitteln
        X = VBMath.Rnd

        A = (5 - 1) * X + 1
        'Console.WriteLine(A)

        'Für jeden der A Hindernisblocks:
        For i = 1 To A

            'Größe G des Hindernisblocks zufällig ermitteln
            X = VBMath.Rnd

            G = (9 - 1) * X + 1
            'console.WriteLine("G: " & G)

            'Startposition P des Hindernisblocks zufällig ermitteln
            X = VBMath.Rnd

            P = (79 - 0) * X + 0
            'Console.WriteLine("P: " & P)

            'Für jedes der G Einzelhindernisse:
            For j = 1 To G

                'Prüfen ob Hinderniss innerhalb des Wertebereichs ist
                If P + j - 1 <= 79 Then

                    'Hinderniss an Position P+j-1 in den Zeilenvektor eintragen
                    Zeile(P + j - 1) = "X"

                End If

            Next

        Next

        ''Ausgabe zum Test
        'For i = 0 To 79
        '    Console.Write(Zeile(i))
        'Next
        'Console.WriteLine()


    End Sub

    Sub Spielablauf()
        Dim leben As Integer
        Dim spielfeld(24, 79) As Char
        Dim Zeile(79) As Char
        Dim z As Integer
        Dim s As Integer
        Dim taste As Integer
        Dim spielfigur_spalte As Integer

        'Startwerte setzen
        leben = 5
        spielfigur_spalte = 39


        'Hauptschleife des Spiels
        Do
            'neue Zeile erzeugen
            ZeilenErzeugung(Zeile)

            'Alle Zeilen des Spielfelds um eine Zeile nach unten verschieben
            'Rückwärtschleife über zeilen
            For z = 24 To 1 Step -1
                'Vorwärtschleife über Spalten
                For s = 0 To 79
                    'Eine Zelle nach unten kopieren
                    spielfeld(z, s) = spielfeld(z - 1, s)

                Next
            Next
            'Neue Zeile am oberen Rand des Spielfelds einfügen
            For s = 0 To 79
                spielfeld(0, s) = Zeile(s)
            Next

            'Spielfeld auf der Konsole ausgeben
            Console.SetCursorPosition(0, 0)
            For z = 0 To 22
                For s = 0 To 79
                    Console.Write(spielfeld(z, s))
                Next
                Console.WriteLine()
            Next

            'Zählschleife für schnelle Bewegung:
            For i = 1 To 10
                'Tastatur abfragen:
                taste = Tastatur_Abfrage()
                'Console.WriteLine(taste)

                'alte Spielfigur löschen:
                Console.SetCursorPosition(spielfigur_spalte, 23)  'Zeile und Spalte anders als bei Matrix
                Console.Write(" ")

                'Position der Spielfigur berechnen:

                If taste = CURSOR_LEFT Then
                    spielfigur_spalte = spielfigur_spalte - 1
                    If spielfigur_spalte < 0 Then
                        spielfigur_spalte = 0
                    End If
                End If



                If taste = CURSOR_RIGHT Then
                    spielfigur_spalte = spielfigur_spalte + 1
                    If spielfigur_spalte > 79 Then
                        spielfigur_spalte = 79
                    End If
                End If

                'Spielfigur auf der Konsole ausgeben: 
                Console.SetCursorPosition(spielfigur_spalte, 23)  'Zeile und Spalte anders als bei Matrix
                Console.Write("#")
            Next

            'Warten
            Threading.Thread.Sleep(200 / 10)

            'Tastaturpuffer leeren:
            Do
                taste = Tastatur_Abfrage()
            Loop Until taste = NO_KEY


        Loop Until leben = 0


    End Sub

    Sub Main()

        Console.CursorVisible = False
        Spielablauf()

    End Sub

End Module