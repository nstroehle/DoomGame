Module Module1

    '  KONSTANTEN – Tastatur
    Const NO_KEY = 0
    Const CURSOR_LEFT = 1
    Const CURSOR_RIGHT = 2
    Const CURSOR_UP = 3
    Const CURSOR_DOWN = 4
    Const KEY_ENTER = 5
    Const KEY_ESCAPE = 6
    Const UNKNOWN_KEY = 99

    '  KONSTANTEN – Spielfeld
    Const SPALTE_MAX = 79
    Const ZEILE_MAX = 24
    Const A_MIN = 1
    Const A_MAX_START = 2
    Const G_MIN = 1
    Const G_MAX = 9
    Const P_MIN = 0
    Const P_MAX = SPALTE_MAX

    Const BEWEGUNG_SPIELFIGUR = 10

    '  KONSTANTEN – Highscore-Datei
    Const HIGHSCORE_DATEI = "highscores.txt"
    Const HIGHSCORE_MAX = 5   ' Maximale Anzahl gespeicherter Einträge

    '  DATENSTRUKTUR – Ein Highscore-Eintrag
    Structure HighscoreEintrag
        Dim Name As String
        Dim Punkte As Integer
    End Structure

    '  FUNKTION: Tastatur_Abfrage
    '  Liest eine Taste nicht-blockierend aus.
    '  Gibt eine der Konstanten zurück.
    Function Tastatur_Abfrage() As Integer
        Dim cki As New ConsoleKeyInfo()
        If Console.KeyAvailable = False Then
            Return NO_KEY
        Else
            cki = Console.ReadKey(True)
            Select Case cki.Key
                Case ConsoleKey.LeftArrow : Return CURSOR_LEFT
                Case ConsoleKey.RightArrow : Return CURSOR_RIGHT
                Case ConsoleKey.UpArrow : Return CURSOR_UP
                Case ConsoleKey.DownArrow : Return CURSOR_DOWN
                Case ConsoleKey.Enter : Return KEY_ENTER
                Case ConsoleKey.Escape : Return KEY_ESCAPE
                Case Else : Return UNKNOWN_KEY
            End Select
        End If
    End Function

    '  FUNKTION: Tastatur_Abfrage_Blockierend
    '  Wartet, bis eine Taste gedrückt wird (für Menüs).
    Function Tastatur_Abfrage_Blockierend() As Integer
        Dim taste As Integer
        Do
            taste = Tastatur_Abfrage()
            Threading.Thread.Sleep(20)
        Loop Until taste <> NO_KEY
        Return taste
    End Function

    '  SUB: Lade_Highscores
    '  Liest die Highscore-Datei ein und füllt das übergebene Array.
    '  Format pro Zeile:  Name;Punkte
    Sub Lade_Highscores(ByRef eintraege() As HighscoreEintrag, ByRef anzahl As Integer)
        anzahl = 0

        ' Prüfen, ob die Datei überhaupt existiert
        If Not System.IO.File.Exists(HIGHSCORE_DATEI) Then
            Return
        End If

        Try
            Dim zeilen() As String = System.IO.File.ReadAllLines(HIGHSCORE_DATEI)
            For Each zeile As String In zeilen
                If anzahl >= HIGHSCORE_MAX Then Exit For

                ' Zeile aufteilen: "Name;1234"
                Dim teile() As String = zeile.Split(";"c)
                If teile.Length = 2 Then
                    eintraege(anzahl).Name = teile(0)
                    eintraege(anzahl).Punkte = CInt(teile(1))
                    anzahl += 1
                End If
            Next
        Catch
            ' Fehler beim Lesen → einfach leer lassen
        End Try
    End Sub

    '  SUB: Speichere_Highscores
    '  Schreibt das Highscore-Array in die Datei.
    Sub Speichere_Highscores(ByRef eintraege() As HighscoreEintrag, ByVal anzahl As Integer)
        Try
            Dim zeilen(anzahl - 1) As String
            For i As Integer = 0 To anzahl - 1
                zeilen(i) = eintraege(i).Name & ";" & eintraege(i).Punkte
            Next
            System.IO.File.WriteAllLines(HIGHSCORE_DATEI, zeilen)
        Catch
            ' Fehler beim Schreiben → ignorieren
        End Try
    End Sub

    '  SUB: Pruefe_Highscore
    '  Wird nach dem Spielende aufgerufen.
    '  Prüft ob der neue Score unter die Top 5 kommt.
    '  Falls ja → Name eingeben → Liste aktualisieren → speichern.
    Sub Pruefe_Highscore(ByVal neuerScore As Integer)
        Dim eintraege(HIGHSCORE_MAX - 1) As HighscoreEintrag
        Dim anzahl As Integer

        Lade_Highscores(eintraege, anzahl)

        ' Prüfen ob Platz frei ist ODER neuer Score besser als schlechtester Eintrag
        Dim istInTop5 As Boolean = False
        If anzahl < HIGHSCORE_MAX Then
            istInTop5 = True
        ElseIf neuerScore > eintraege(anzahl - 1).Punkte Then
            istInTop5 = True
        End If

        If Not istInTop5 Then Return

        ' Name des Spielers abfragen
        Console.Clear()
        Console.ForegroundColor = ConsoleColor.Yellow
        Console.SetCursorPosition(20, 10)
        Console.WriteLine("*** NEUER HIGHSCORE: " & neuerScore & " ***")
        Console.SetCursorPosition(20, 12)
        Console.Write("Dein Name: ")
        Console.CursorVisible = True
        Dim spielerName As String = Console.ReadLine()
        Console.CursorVisible = False

        ' Sicherheit: leerer Name → "???"
        If spielerName.Trim() = "" Then spielerName = "???"
        ' Auf 10 Zeichen kürzen
        If spielerName.Length > 10 Then spielerName = spielerName.Substring(0, 10)

        ' Neuen Eintrag einfügen (falls Liste noch nicht voll)
        If anzahl < HIGHSCORE_MAX Then
            eintraege(anzahl).Name = spielerName
            eintraege(anzahl).Punkte = neuerScore
            anzahl += 1
        Else
            ' Schlechtesten überschreiben (letzter Platz)
            eintraege(anzahl - 1).Name = spielerName
            eintraege(anzahl - 1).Punkte = neuerScore
        End If

        ' Liste absteigend sortieren (einfacher Bubble-Sort)
        Dim getauscht As Boolean
        Do
            getauscht = False
            For i As Integer = 0 To anzahl - 2
                If eintraege(i).Punkte < eintraege(i + 1).Punkte Then
                    Dim temp As HighscoreEintrag = eintraege(i)
                    eintraege(i) = eintraege(i + 1)
                    eintraege(i + 1) = temp
                    getauscht = True
                End If
            Next
        Loop While getauscht

        ' Speichern
        Speichere_Highscores(eintraege, anzahl)
    End Sub

    '  SUB: Zeige_Highscores
    '  Zeigt die Top-5-Liste im Doom-Stil an.
    '  Verlassen mit beliebiger Taste.
    Sub Zeige_Highscores()
        Dim eintraege(HIGHSCORE_MAX - 1) As HighscoreEintrag
        Dim anzahl As Integer

        Lade_Highscores(eintraege, anzahl)

        Console.Clear()
        Console.BackgroundColor = ConsoleColor.Black
        Console.ForegroundColor = ConsoleColor.Red

        ' Titel-Banner
        Console.SetCursorPosition(20, 2)
        Console.WriteLine("##################################")
        Console.SetCursorPosition(20, 3)
        Console.WriteLine("#       -- HIGHSCORES --         #")
        Console.SetCursorPosition(20, 4)
        Console.WriteLine("##################################")

        Console.ForegroundColor = ConsoleColor.DarkYellow
        Console.SetCursorPosition(20, 6)
        Console.WriteLine("  #   Name            Punkte")
        Console.SetCursorPosition(20, 7)
        Console.WriteLine("  -   ----------      --------")

        ' Einträge anzeigen
        If anzahl = 0 Then
            Console.ForegroundColor = ConsoleColor.Gray
            Console.SetCursorPosition(20, 9)
            Console.WriteLine("  Noch keine Eintraege vorhanden.")
        Else
            For i As Integer = 0 To anzahl - 1
                Console.ForegroundColor = ConsoleColor.White

                ' Platz 1 in Gold hervorheben
                If i = 0 Then Console.ForegroundColor = ConsoleColor.Yellow

                Console.SetCursorPosition(20, 9 + i)
                Console.WriteLine("  " & (i + 1) & ".  " &
                                  eintraege(i).Name.PadRight(16) &
                                  eintraege(i).Punkte.ToString().PadLeft(8))
            Next
        End If

        Console.ForegroundColor = ConsoleColor.DarkGray
        Console.SetCursorPosition(20, 16)
        Console.WriteLine("  [Beliebige Taste] Zurueck zum Menue")

        ' Warten bis Taste gedrückt
        Tastatur_Abfrage_Blockierend()
    End Sub

    '  SUB: Zeige_Hauptmenue
    '  Zeigt das Hauptmenü mit ↑/↓ Navigation und Enter-Auswahl.
    '  Gibt die gewählte Option zurück:
    '    1 = Einzelspieler starten
    '    2 = Highscores anzeigen
    '    3 = Beenden
    Function Zeige_Hauptmenue() As Integer
        Dim auswahl As Integer = 1      ' Aktuelle Menüauswahl (1-3)
        Dim taste As Integer
        Dim weiter As Boolean = True

        Do While weiter
            ' ---- Bildschirm aufbauen ----
            Console.Clear()
            Console.BackgroundColor = ConsoleColor.Black
            Console.ForegroundColor = ConsoleColor.DarkRed

            ' ASCII-Art Titel (Doom-Stil)
            Console.SetCursorPosition(5, 2)
            Console.WriteLine(" 
     ________            ______  ___
     ___  __ \______________   |/  /
     __  / / /  __ \  __ \_  /|_/ / 
     _  /_/ // /_/ / /_/ /  /  / /  
     /_____/ \____/\____//_/  /_/   
                               ")


            Console.ForegroundColor = ConsoleColor.DarkGray
            Console.SetCursorPosition(5, 9)
            Console.WriteLine(" -- CONSOLE EDITION --")

            ' ---- Menüpunkte zeichnen ----
            Dim menuePunkte() As String = {"Einzelspieler", "Highscores", "Beenden"}

            For i As Integer = 0 To menuePunkte.Length - 1
                Console.SetCursorPosition(28, 11 + i * 2)

                If i + 1 = auswahl Then
                    ' Aktiver Eintrag: rot + Pfeil
                    Console.ForegroundColor = ConsoleColor.Red
                    Console.Write("> " & menuePunkte(i) & " <")
                Else
                    ' Inaktiver Eintrag: grau
                    Console.ForegroundColor = ConsoleColor.DarkGray
                    Console.Write("  " & menuePunkte(i) & "  ")
                End If
            Next

            ' Steuerungshinweis
            Console.ForegroundColor = ConsoleColor.DarkGray
            Console.SetCursorPosition(22, 18)
            Console.WriteLine("[Pfeiltasten] Navigieren   [Enter] Auswaehlen")

            ' ---- Auf Eingabe warten ----
            taste = Tastatur_Abfrage_Blockierend()

            Select Case taste
                Case CURSOR_UP
                    auswahl -= 1
                    If auswahl < 1 Then auswahl = menuePunkte.Length   ' Wrap-around nach oben

                Case CURSOR_DOWN
                    auswahl += 1
                    If auswahl > menuePunkte.Length Then auswahl = 1   ' Wrap-around nach unten

                Case KEY_ENTER
                    weiter = False   ' Auswahl getroffen → Schleife verlassen
            End Select
        Loop

        Return auswahl
    End Function

    '  SUB: Erzeuge_Zeile  (unverändert aus Basislösung)
    Sub Erzeuge_Zeile(ByRef Zeile() As Char, ByVal a_max As Integer)
        Dim a As Integer
        Dim x As Single
        Dim i, j As Integer
        Dim g As Integer
        Dim p As Integer

        For i = 0 To SPALTE_MAX
            Zeile(i) = " "
        Next

        Randomize()
        x = VBMath.Rnd
        a = (a_max - A_MIN) * x + A_MIN

        For i = 1 To a
            Randomize()
            x = VBMath.Rnd
            g = (G_MAX - G_MIN) * x + G_MIN

            Randomize()
            x = VBMath.Rnd
            p = (P_MAX - P_MIN) * x + P_MIN

            For j = 1 To g
                If p + j - 1 <= SPALTE_MAX Then
                    Zeile(p + j - 1) = "X"
                End If
            Next
        Next
    End Sub

    '  SUB: Game_Over  (leicht angepasst: gibt Score zurück)
    Sub Game_Over(ByVal score As Integer)
        Console.BackgroundColor = ConsoleColor.Red
        Console.ForegroundColor = ConsoleColor.White
        Console.Clear()

        Console.SetCursorPosition(10, 8)
        Console.WriteLine("  ___  _   __  __ ___    _____   _____ ___  ")
        Console.SetCursorPosition(10, 9)
        Console.WriteLine(" / __|| | / _||  V  || __| \ / / __| _ \ ")
        Console.SetCursorPosition(10, 10)
        Console.WriteLine("| (_ || || |_ | \_/ || _|   V /| _||   / ")
        Console.SetCursorPosition(10, 11)
        Console.WriteLine(" \___||_| \__||_| |_||___|  \_/ |___|_|_\ ")

        Console.ForegroundColor = ConsoleColor.Yellow
        Console.SetCursorPosition(30, 14)
        Console.WriteLine("Dein Score: " & score)

        Console.ForegroundColor = ConsoleColor.White
        Console.SetCursorPosition(25, 16)
        Console.WriteLine("[Enter] Weiter zum Menue")

        ' Warten bis Enter gedrückt
        Dim taste As Integer
        Do
            taste = Tastatur_Abfrage_Blockierend()
        Loop Until taste = KEY_ENTER
    End Sub

    '  SUB: Spielablauf  (unverändert – Score wird mitgezählt,
    '  aber noch nicht angezeigt; kommt in Schritt 2)
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
        Dim score As Integer   ' NEU: Punktezähler

        ' Startwerte setzen
        leben = 5
        score = 0
        spielfigur_spalte = SPALTE_MAX / 2
        wartezeit = 200
        a_max = A_MAX_START

        ' Hauptspielschleife
        Do
            Erzeuge_Zeile(zeile, a_max)

            ' Spielfeld nach unten verschieben
            For z = ZEILE_MAX To 1 Step -1
                For s = 0 To SPALTE_MAX
                    spielfeld(z, s) = spielfeld(z - 1, s)
                Next
            Next

            ' Neue Zeile oben eintragen
            For s = 0 To SPALTE_MAX
                spielfeld(0, s) = zeile(s)
            Next

            ' Spielfeld ausgeben
            Console.SetCursorPosition(0, 0)
            For z = 0 To ZEILE_MAX - 2
                For s = 0 To SPALTE_MAX
                    Console.Write(spielfeld(z, s))
                Next
                Console.WriteLine()
            Next

            ' Schnelle Bewegungsschleife
            For i = 1 To BEWEGUNG_SPIELFIGUR
                taste = Tastatur_Abfrage()

                ' Alte Spielfigur löschen
                Console.SetCursorPosition(spielfigur_spalte, ZEILE_MAX - 1)
                Console.Write(" ")

                ' Position berechnen
                If taste = CURSOR_LEFT Then
                    spielfigur_spalte -= 1
                    If spielfigur_spalte < 0 Then spielfigur_spalte = 0
                End If
                If taste = CURSOR_RIGHT Then
                    spielfigur_spalte += 1
                    If spielfigur_spalte > SPALTE_MAX Then spielfigur_spalte = SPALTE_MAX
                End If

                ' Spielfigur ausgeben
                Console.SetCursorPosition(spielfigur_spalte, ZEILE_MAX - 1)
                Console.Write("#")

                ' Kollisionsprüfung
                If spielfeld(22, spielfigur_spalte) = "X" Then
                    leben -= 1
                    Console.Beep()
                    spielfeld(22, spielfigur_spalte) = " "
                End If

                ' HUD: Leben + Score
                Console.SetCursorPosition(0, ZEILE_MAX)
                Console.Write("Leben: " & leben & "  Score: " & score & "   ")

                Threading.Thread.Sleep(wartezeit / BEWEGUNG_SPIELFIGUR)
            Next

            ' Score pro überlebter Runde +10
            score += 10

            ' Tastaturpuffer leeren
            Do
                taste = Tastatur_Abfrage()
            Loop Until taste = NO_KEY

            ' Schwierigkeit erhöhen
            wartezeit = wartezeit * 0.99
            If wartezeit < 0 Then wartezeit = 0
            a_max = a_max * 1.03

        Loop Until leben <= 0

        ' Spielende
        Game_Over(score)
        Pruefe_Highscore(score)
    End Sub

    '  SUB: Main – Einstiegspunkt
    '  Zeigt das Hauptmenü und reagiert auf die Auswahl.
    Sub Main()
        Console.CursorVisible = False
        Console.Title = "DOOM Console Edition"

        Dim laeuft As Boolean = True

        Do While laeuft
            Dim auswahl As Integer = Zeige_Hauptmenue()

            Select Case auswahl
                Case 1
                    ' Einzelspieler starten
                    Console.Clear()
                    Spielablauf()

                Case 2
                    ' Highscores anzeigen
                    Zeige_Highscores()

                Case 3
                    ' Beenden
                    laeuft = False
            End Select
        Loop

        ' Abschlussbildschirm
        Console.Clear()
        Console.ForegroundColor = ConsoleColor.DarkRed
        Console.SetCursorPosition(25, 12)
        Console.WriteLine("Bis zum naechsten Mal, Marine.")
        Threading.Thread.Sleep(1500)
    End Sub

End Module