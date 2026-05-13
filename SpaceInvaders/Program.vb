Module Module1

    '  KONSTANTEN – Tastatur
    Const NO_KEY = 0
    Const CURSOR_LEFT = 1
    Const CURSOR_RIGHT = 2
    Const CURSOR_UP = 3
    Const CURSOR_DOWN = 4
    Const KEY_ENTER = 5
    Const KEY_ESCAPE = 6
    Const KEY_SPACE = 7        ' NEU: Leertaste zum Schießen
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

    ' Wie viele Bewegungs-Ticks pro Spielfeld-Vorschub
    Const BEWEGUNG_SPIELFIGUR = 10

    '  KONSTANTEN – Spielfigur & Schuss
    ' Das Player-Model besteht aus 3 Zeichen nebeneinander: (^)
    ' spielfigur_spalte zeigt immer auf das MITTLERE Zeichen (^)
    Const PLAYER_LINKS As Char = "("c     ' Linkes Bauteil des Spielers
    Const PLAYER_MITTE As Char = "^"c     ' Mittelteil / Kanone
    Const PLAYER_RECHTS As Char = ")"c    ' Rechtes Bauteil des Spielers

    ' Schuss-Zeichen (fliegt nach oben)
    Const SCHUSS_ZEICHEN As Char = "|"c

    ' Zeile, auf der der Spieler steht (unterste Spielzeile)
    Const SPIELER_ZEILE = ZEILE_MAX - 1   ' = Zeile 23

    ' Maximale gleichzeitige Schüsse
    Const MAX_SCHUESSE = 3

    ' Ammo (Munition) pro Spiel – wird nicht regeneriert, aber Schüsse kosten 1
    Const AMMO_START = 30

    ' Punkte für abgeschossenes Hindernis
    Const PUNKTE_ABSCHUSS = 25

    '  KONSTANTEN – Hindernisse (Doom-Stil)
    Const HINDERNIS_ZEICHEN As Char = "W"c

    '  KONSTANTEN – Highscore-Datei
    Const HIGHSCORE_DATEI = "highscores.txt"
    Const HIGHSCORE_MAX = 5

    '  DATENSTRUKTUR – Highscore-Eintrag
    Structure HighscoreEintrag
        Dim Name As String
        Dim Punkte As Integer
    End Structure

    '  DATENSTRUKTUR – Ein einzelner Schuss
    Structure Schuss
        Dim Spalte As Integer   ' Horizontale Position
        Dim Zeile As Integer    ' Vertikale Position (bewegt sich nach oben = Zeile -1)
        Dim Aktiv As Boolean    ' Ist dieser Schuss gerade im Flug?
    End Structure

    '  ABSCHNITT 1: TASTATUR

    ' FUNKTION: Tastatur_Abfrage
    ' Liest eine Taste nicht-blockierend aus.
    ' Gibt eine der Konstanten zurück.
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
                Case ConsoleKey.Spacebar : Return KEY_SPACE   ' NEU: Leertaste
                Case Else : Return UNKNOWN_KEY
            End Select
        End If
    End Function

    ' FUNKTION: Tastatur_Abfrage_Blockierend
    ' Wartet solange, bis eine Taste gedrückt wird (für Menüs).
    Function Tastatur_Abfrage_Blockierend() As Integer
        Dim taste As Integer
        Do
            taste = Tastatur_Abfrage()
            Threading.Thread.Sleep(20)
        Loop Until taste <> NO_KEY
        Return taste
    End Function

    '  ABSCHNITT 2: HIGHSCORE (Laden / Speichern / Prüfen / Anzeigen)

    ' SUB: Lade_Highscores
    ' Liest die Highscore-Datei ein. Format pro Zeile: Name;Punkte
    Sub Lade_Highscores(ByRef eintraege() As HighscoreEintrag, ByRef anzahl As Integer)
        anzahl = 0

        ' Datei existiert noch nicht → leer lassen
        If Not System.IO.File.Exists(HIGHSCORE_DATEI) Then Return

        Try
            Dim zeilen() As String = System.IO.File.ReadAllLines(HIGHSCORE_DATEI)
            For Each zeile As String In zeilen
                If anzahl >= HIGHSCORE_MAX Then Exit For

                ' Zeile aufteilen: "Spieler;1234"
                Dim teile() As String = zeile.Split(";"c)
                If teile.Length = 2 Then
                    eintraege(anzahl).Name = teile(0)
                    eintraege(anzahl).Punkte = CInt(teile(1))
                    anzahl += 1
                End If
            Next
        Catch
            ' Lesefehler → einfach leer lassen
        End Try
    End Sub

    ' SUB: Speichere_Highscores
    ' Schreibt das Highscore-Array zurück in die Datei.
    Sub Speichere_Highscores(ByRef eintraege() As HighscoreEintrag, ByVal anzahl As Integer)
        Try
            Dim zeilen(anzahl - 1) As String
            For i As Integer = 0 To anzahl - 1
                zeilen(i) = eintraege(i).Name & ";" & eintraege(i).Punkte
            Next
            System.IO.File.WriteAllLines(HIGHSCORE_DATEI, zeilen)
        Catch
            ' Schreibfehler → ignorieren
        End Try
    End Sub

    ' SUB: Pruefe_Highscore
    ' Wird nach Spielende aufgerufen.
    ' Wenn der Score in die Top 5 gehört → Name eingeben → sortieren → speichern.
    Sub Pruefe_Highscore(ByVal neuerScore As Integer)
        Dim eintraege(HIGHSCORE_MAX - 1) As HighscoreEintrag
        Dim anzahl As Integer

        Lade_Highscores(eintraege, anzahl)

        ' Ist der Score gut genug für die Top 5?
        Dim istInTop5 As Boolean = False
        If anzahl < HIGHSCORE_MAX Then
            istInTop5 = True   ' Liste noch nicht voll → immer rein
        ElseIf neuerScore > eintraege(anzahl - 1).Punkte Then
            istInTop5 = True   ' Besser als der schlechteste Eintrag
        End If

        If Not istInTop5 Then Return   ' Nicht in Top 5 → nichts tun

        ' ---- Name eingeben ----
        Console.Clear()
        Console.ForegroundColor = ConsoleColor.Yellow
        Console.SetCursorPosition(20, 10)
        Console.WriteLine("*** NEUER HIGHSCORE: " & neuerScore & " Punkte! ***")
        Console.SetCursorPosition(20, 12)
        Console.Write("Dein Name (max. 10 Zeichen): ")
        Console.CursorVisible = True
        Dim spielerName As String = Console.ReadLine()
        Console.CursorVisible = False

        ' Leerstring absichern
        If spielerName.Trim() = "" Then spielerName = "Marine"
        If spielerName.Length > 10 Then spielerName = spielerName.Substring(0, 10)

        ' ---- Eintrag in Liste aufnehmen ----
        If anzahl < HIGHSCORE_MAX Then
            ' Liste noch nicht voll → hinten anhängen
            eintraege(anzahl).Name = spielerName
            eintraege(anzahl).Punkte = neuerScore
            anzahl += 1
        Else
            ' Liste voll → schlechtesten Eintrag überschreiben
            eintraege(anzahl - 1).Name = spielerName
            eintraege(anzahl - 1).Punkte = neuerScore
        End If

        ' ---- Liste absteigend sortieren (Bubble-Sort) ----
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

        ' ---- Speichern ----
        Speichere_Highscores(eintraege, anzahl)
    End Sub

    ' SUB: Zeige_Highscores
    Sub Zeige_Highscores()
        Dim eintraege(HIGHSCORE_MAX - 1) As HighscoreEintrag
        Dim anzahl As Integer

        Lade_Highscores(eintraege, anzahl)

        Console.Clear()
        Console.BackgroundColor = ConsoleColor.Black
        Console.ForegroundColor = ConsoleColor.Red

        ' Titel-Banner
        Console.SetCursorPosition(22, 2)
        Console.WriteLine("################################")
        Console.SetCursorPosition(22, 3)
        Console.WriteLine("#      -- HALL OF DOOM --      #")
        Console.SetCursorPosition(22, 4)
        Console.WriteLine("################################")

        Console.ForegroundColor = ConsoleColor.DarkYellow
        Console.SetCursorPosition(22, 6)
        Console.WriteLine("  #   Name            Punkte")
        Console.SetCursorPosition(22, 7)
        Console.WriteLine("  -   ----------      --------")

        ' Einträge anzeigen
        If anzahl = 0 Then
            Console.ForegroundColor = ConsoleColor.Gray
            Console.SetCursorPosition(22, 9)
            Console.WriteLine("  Noch keine Eintraege vorhanden.")
        Else
            For i As Integer = 0 To anzahl - 1
                ' Platz 1 golden, Rest weiß
                If i = 0 Then
                    Console.ForegroundColor = ConsoleColor.Yellow
                Else
                    Console.ForegroundColor = ConsoleColor.White
                End If

                Console.SetCursorPosition(22, 9 + i)
                Console.WriteLine("  " & (i + 1) & ".  " &
                                  eintraege(i).Name.PadRight(16) &
                                  eintraege(i).Punkte.ToString().PadLeft(8))
            Next
        End If

        Console.ForegroundColor = ConsoleColor.DarkGray
        Console.SetCursorPosition(22, 17)
        Console.WriteLine("  [Beliebige Taste] Zurueck zum Menue")

        Tastatur_Abfrage_Blockierend()
    End Sub

    '  ABSCHNITT 3: HAUPTMENÜ

    ' FUNKTION: Zeige_Hauptmenue
    ' Navigation mit ↑/↓, Auswahl mit Enter.
    ' Rückgabe: 1 = Spielen, 2 = Highscores, 3 = Beenden
    Function Zeige_Hauptmenue() As Integer
        Dim auswahl As Integer = 1
        Dim taste As Integer
        Dim weiter As Boolean = True

        Do While weiter
            Console.Clear()
            Console.BackgroundColor = ConsoleColor.Black
            Console.ForegroundColor = ConsoleColor.DarkRed

            ' ---- ASCII-Art Titel (Doom-Stil) ----
            Console.SetCursorPosition(5, 1)
            Console.WriteLine("  
     ▓█████▄  ▒█████   ▒█████   ███▄ ▄███▓
     ▒██▀ ██▌▒██▒  ██▒▒██▒  ██▒▓██▒▀█▀ ██▒
     ░██   █▌▒██░  ██▒▒██░  ██▒▓██    ▓██░
     ░▓█▄   ▌▒██   ██░▒██   ██░▒██    ▒██ 
     ░▒████▓ ░ ████▓▒░░ ████▓▒░▒██▒   ░██▒
     ▒▒▓  ▒ ░ ▒░▒░▒░ ░ ▒░▒░▒░ ░ ▒░   ░  ░
     ░ ▒  ▒   ░ ▒ ▒░   ░ ▒ ▒░ ░  ░      ░
     ░ ░  ░ ░ ░ ░ ▒  ░ ░ ░ ▒  ░      ░   
      ░        ░ ░      ░ ░         ░   
     ░                                   ")

            Console.ForegroundColor = ConsoleColor.DarkGray
            Console.SetCursorPosition(5, 12)
            Console.WriteLine(" -- CONSOLE EDITION --  Einzelspieler")

            ' ---- Menüpunkte ----
            Dim menuePunkte() As String = {"  Spielen starten  ",
                                           "  Hall of Doom     ",
                                           "  Beenden          "}

            For i As Integer = 0 To menuePunkte.Length - 1
                Console.SetCursorPosition(26, 15 + i * 2)

                If i + 1 = auswahl Then
                    ' Aktiver Eintrag: rot mit Pfeil
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
            Console.SetCursorPosition(20, 22)
            Console.WriteLine("[Pfeiltasten] Navigieren   [Enter] Auswaehlen")

            ' ---- Tasteneingabe ----
            taste = Tastatur_Abfrage_Blockierend()

            Select Case taste
                Case CURSOR_UP
                    auswahl -= 1
                    If auswahl < 1 Then auswahl = menuePunkte.Length

                Case CURSOR_DOWN
                    auswahl += 1
                    If auswahl > menuePunkte.Length Then auswahl = 1

                Case KEY_ENTER
                    weiter = False
            End Select
        Loop

        Return auswahl
    End Function

    '  ABSCHNITT 4: SPIELFELD-LOGIK

    ' SUB: Erzeuge_Zeile
    ' Generiert eine neue Zeile mit zufälligen Hindernissen (HINDERNIS_ZEICHEN).
    Sub Erzeuge_Zeile(ByRef Zeile() As Char, ByVal a_max As Integer)
        Dim a As Integer
        Dim x As Single
        Dim i, j As Integer
        Dim g As Integer
        Dim p As Integer

        ' Zeile zunächst mit Leerzeichen füllen
        For i = 0 To SPALTE_MAX
            Zeile(i) = " "c
        Next

        ' Zufällige Anzahl an Hindernisgruppen (a)
        Randomize()
        x = VBMath.Rnd
        a = CInt((a_max - A_MIN) * x + A_MIN)

        ' Für jede Hindernisgruppe: Größe (g) und Position (p) zufällig
        For i = 1 To a
            Randomize()
            x = VBMath.Rnd
            g = CInt((G_MAX - G_MIN) * x + G_MIN)

            Randomize()
            x = VBMath.Rnd
            p = CInt((P_MAX - P_MIN) * x + P_MIN)

            ' Hindernisgruppe ins Array schreiben
            For j = 1 To g
                If p + j - 1 <= SPALTE_MAX Then
                    Zeile(p + j - 1) = HINDERNIS_ZEICHEN
                End If
            Next
        Next
    End Sub

    '  ABSCHNITT 5: SCHUSS-MECHANIK

    ' SUB: Feuere_Schuss
    ' Aktiviert einen freien Schuss-Slot und setzt ihn direkt über den Spieler.
    ' Gibt True zurück wenn ein Schuss abgefeuert wurde, False wenn kein Slot frei.
    Function Feuere_Schuss(ByRef schuesse() As Schuss, ByVal spieler_spalte As Integer) As Boolean
        ' Freien Slot suchen
        For i As Integer = 0 To MAX_SCHUESSE - 1
            If Not schuesse(i).Aktiv Then
                schuesse(i).Spalte = spieler_spalte     ' Gleiche Spalte wie Spieler (Mitte)
                schuesse(i).Zeile = SPIELER_ZEILE - 1   ' Direkt über dem Spieler starten
                schuesse(i).Aktiv = True
                Return True   ' Schuss erfolgreich abgefeuert
            End If
        Next
        Return False   ' Kein freier Slot → kein Schuss
    End Function

    ' SUB: Aktualisiere_Schuesse
    ' Bewegt alle aktiven Schüsse eine Zeile nach oben.
    ' Prüft auf Kollision mit Hindernissen im Spielfeld.
    ' Bei Treffer: Hindernis löschen + Punkte vergeben.
    Sub Aktualisiere_Schuesse(ByRef schuesse() As Schuss,
                              ByRef spielfeld(,) As Char,
                              ByRef score As Integer)

        For i As Integer = 0 To MAX_SCHUESSE - 1
            If schuesse(i).Aktiv Then

                ' Altes Schuss-Zeichen auf dem Bildschirm löschen
                If schuesse(i).Zeile >= 0 AndAlso schuesse(i).Zeile < ZEILE_MAX - 1 Then
                    Console.SetCursorPosition(schuesse(i).Spalte, schuesse(i).Zeile)
                    Console.Write(" ")
                End If

                ' Schuss eine Zeile nach oben bewegen
                schuesse(i).Zeile -= 1

                ' Schuss hat das obere Ende des Spielfelds erreicht → deaktivieren
                If schuesse(i).Zeile < 0 Then
                    schuesse(i).Aktiv = False
                    Continue For
                End If

                ' ---- Kollisionsprüfung ----
                ' Trifft der Schuss an seiner neuen Position ein Hindernis?
                If spielfeld(schuesse(i).Zeile, schuesse(i).Spalte) = HINDERNIS_ZEICHEN Then
                    ' Treffer! Hindernis im Spielfeld löschen
                    spielfeld(schuesse(i).Zeile, schuesse(i).Spalte) = " "c

                    ' Treffer-Position auf Bildschirm löschen
                    Console.SetCursorPosition(schuesse(i).Spalte, schuesse(i).Zeile)
                    Console.Write(" ")

                    ' Punkte für Abschuss vergeben
                    score += PUNKTE_ABSCHUSS

                    ' Schuss deaktivieren (hat getroffen)
                    schuesse(i).Aktiv = False
                Else
                    ' Kein Treffer → Schuss-Zeichen an neuer Position zeichnen
                    Console.ForegroundColor = ConsoleColor.Cyan
                    Console.SetCursorPosition(schuesse(i).Spalte, schuesse(i).Zeile)
                    Console.Write(SCHUSS_ZEICHEN)
                    Console.ForegroundColor = ConsoleColor.White
                End If

            End If
        Next
    End Sub

    '  ABSCHNITT 6: SPIELER ZEICHNEN / LÖSCHEN

    ' SUB: Zeichne_Spieler
    ' Gibt das 3-teilige Player-Model an der aktuellen Spalte aus.
    ' Das Modell sieht so aus:  (^)
    '   Links  = "("
    '   Mitte  = "^"  ← spielfigur_spalte zeigt hierauf
    '   Rechts = ")"
    Sub Zeichne_Spieler(ByVal spalte As Integer)
        Console.ForegroundColor = ConsoleColor.Green

        ' Linkes Teil (nur wenn nicht am Rand)
        If spalte - 1 >= 0 Then
            Console.SetCursorPosition(spalte - 1, SPIELER_ZEILE)
            Console.Write(PLAYER_LINKS)
        End If

        ' Mittelteil (Kanone)
        Console.SetCursorPosition(spalte, SPIELER_ZEILE)
        Console.Write(PLAYER_MITTE)

        ' Rechtes Teil (nur wenn nicht am Rand)
        If spalte + 1 <= SPALTE_MAX Then
            Console.SetCursorPosition(spalte + 1, SPIELER_ZEILE)
            Console.Write(PLAYER_RECHTS)
        End If

        Console.ForegroundColor = ConsoleColor.White
    End Sub

    ' SUB: Loesche_Spieler
    ' Überschreibt das Player-Model mit Leerzeichen (vor Bewegung).
    Sub Loesche_Spieler(ByVal spalte As Integer)
        If spalte - 1 >= 0 Then
            Console.SetCursorPosition(spalte - 1, SPIELER_ZEILE)
            Console.Write(" ")
        End If
        Console.SetCursorPosition(spalte, SPIELER_ZEILE)
        Console.Write(" ")
        If spalte + 1 <= SPALTE_MAX Then
            Console.SetCursorPosition(spalte + 1, SPIELER_ZEILE)
            Console.Write(" ")
        End If
    End Sub

    '  ABSCHNITT 7: HUD (Heads-Up Display)

    ' SUB: Zeichne_HUD
    ' Zeigt unten am Bildschirm: Leben, Score und Ammo an.
    Sub Zeichne_HUD(ByVal leben As Integer, ByVal score As Integer, ByVal ammo As Integer)
        Console.SetCursorPosition(0, ZEILE_MAX)
        Console.ForegroundColor = ConsoleColor.Red
        Console.Write("HP: " & leben.ToString().PadLeft(2) & "  ")

        Console.ForegroundColor = ConsoleColor.Yellow
        Console.Write("Score: " & score.ToString().PadLeft(6) & "  ")

        Console.ForegroundColor = ConsoleColor.Cyan
        Console.Write("Ammo: " & ammo.ToString().PadLeft(3) & "   ")

        Console.ForegroundColor = ConsoleColor.DarkGray
        Console.Write("[<][>] Bewegen  [Leer] Schiessen")

        Console.ForegroundColor = ConsoleColor.White
    End Sub

    '  ABSCHNITT 8: GAME OVER

    ' SUB: Game_Over
    ' Zeigt den roten Game-Over-Bildschirm mit dem erzielten Score.
    Sub Game_Over(ByVal score As Integer)
        Console.BackgroundColor = ConsoleColor.Red
        Console.ForegroundColor = ConsoleColor.White
        Console.Clear()

        Console.SetCursorPosition(10, 7)
        Console.WriteLine("
       ▄████  ▄▄▄       ███▄ ▄███▓▓█████     ▒█████   ██▒   █▓▓█████  ██▀███  
      ██▒ ▀█▒▒████▄    ▓██▒▀█▀ ██▒▓█   ▀    ▒██▒  ██▒▓██░   █▒▓█   ▀ ▓██ ▒ ██▒
     ▒██░▄▄▄░▒██  ▀█▄  ▓██    ▓██░▒███      ▒██░  ██▒ ▓██  █▒░▒███   ▓██ ░▄█ ▒
     ░▓█  ██▓░██▄▄▄▄██ ▒██    ▒██ ▒▓█  ▄    ▒██   ██░  ▒██ █░░▒▓█  ▄ ▒██▀▀█▄  
     ░▒▓███▀▒ ▓█   ▓██▒▒██▒   ░██▒░▒████▒   ░ ████▓▒░   ▒▀█░  ░▒████▒░██▓ ▒██▒
      ░▒   ▒  ▒▒   ▓▒█░░ ▒░   ░  ░░░ ▒░ ░   ░ ▒░▒░▒░    ░ ▐░  ░░ ▒░ ░░ ▒▓ ░▒▓░
       ░   ░   ▒   ▒▒ ░░  ░      ░ ░ ░  ░     ░ ▒ ▒░    ░ ░░   ░ ░  ░  ░▒ ░ ▒░
      ░ ░   ░   ░   ▒   ░      ░      ░      ░ ░ ░ ▒       ░░     ░     ░░   ░ 
         ░       ░  ░       ░      ░  ░       ░ ░        ░     ░  ░   ░     
                                                     ░                   
        ")












        Console.ForegroundColor = ConsoleColor.Yellow
        Console.SetCursorPosition(25, 19)
        Console.WriteLine("Dein Score: " & score & " Punkte")

        Console.ForegroundColor = ConsoleColor.White
        Console.SetCursorPosition(25, 21)
        Console.WriteLine("[Enter] Weiter zum Menue")

        ' Warten bis Enter gedrückt wird
        Dim taste As Integer
        Do
            taste = Tastatur_Abfrage_Blockierend()
        Loop Until taste = KEY_ENTER

        Console.BackgroundColor = ConsoleColor.Black
    End Sub

    '  ABSCHNITT 9: HAUPTSPIELABLAUF

    ' SUB: Spielablauf
    ' Enthält die komplette Spielschleife:
    '   1. Neue Hindernis-Zeile erzeugen
    '   2. Spielfeld nach unten schieben
    '   3. Spielfeld ausgeben
    '   4. Bewegungsschleife: Spieler steuern, Schüsse abfeuern, Schüsse bewegen
    '   5. Kollision Spieler <-> Hindernis prüfen
    '   6. HUD aktualisieren
    '   7. Schwierigkeit steigern
    Sub Spielablauf()

        ' ---- Spielvariablen initialisieren ----
        Dim leben As Integer = 5
        Dim score As Integer = 0
        Dim ammo As Integer = AMMO_START
        Dim spielfigur_spalte As Integer = SPALTE_MAX \ 2   ' Startposition in der Mitte
        Dim wartezeit As Single = 200.0   ' Millisekunden pro Bewegungs-Tick (sinkt mit der Zeit)
        Dim a_max As Single = A_MAX_START ' Maximale Hindernisgruppen pro Zeile (steigt mit der Zeit)

        ' Spielfeld als 2D-Array (Zeilen x Spalten)
        Dim spielfeld(ZEILE_MAX, SPALTE_MAX) As Char
        Dim neueZeile(SPALTE_MAX) As Char

        ' Schuss-Array initialisieren (alle Schüsse inaktiv)
        Dim schuesse(MAX_SCHUESSE - 1) As Schuss
        For i As Integer = 0 To MAX_SCHUESSE - 1
            schuesse(i).Aktiv = False
        Next

        ' Spielfeld zu Beginn mit Leerzeichen füllen
        For z As Integer = 0 To ZEILE_MAX
            For s As Integer = 0 To SPALTE_MAX
                spielfeld(z, s) = " "c
            Next
        Next

        Console.Clear()
        Console.BackgroundColor = ConsoleColor.Black

        '  HAUPTSPIELSCHLEIFE – läuft bis Leben = 0
        Do

            ' SCHRITT 1: Neue Hindernis-Zeile erzeugen
            Erzeuge_Zeile(neueZeile, a_max)

            ' SCHRITT 2: Spielfeld eine Zeile nach unten schieben
            ' (Zeile 0 = oben, daher von unten anfangen)
            For z As Integer = ZEILE_MAX To 1 Step -1
                For s As Integer = 0 To SPALTE_MAX
                    spielfeld(z, s) = spielfeld(z - 1, s)
                Next
            Next

            ' SCHRITT 3: Neue Zeile ganz oben eintragen
            For s As Integer = 0 To SPALTE_MAX
                spielfeld(0, s) = neueZeile(s)
            Next

            ' SCHRITT 4: Spielfeld auf dem Bildschirm ausgeben
            Console.SetCursorPosition(0, 0)
            For z As Integer = 0 To ZEILE_MAX - 2

                For s As Integer = 0 To SPALTE_MAX
                    ' Hindernisse rot einfärben, Rest grau
                    If spielfeld(z, s) = HINDERNIS_ZEICHEN Then
                        Console.ForegroundColor = ConsoleColor.DarkRed
                    Else
                        Console.ForegroundColor = ConsoleColor.DarkGray
                    End If
                    Console.Write(spielfeld(z, s))
                Next

                Console.WriteLine()
            Next
            Console.ForegroundColor = ConsoleColor.White

            ' SCHRITT 5: Bewegungs- und Interaktionsschleife
            ' Läuft BEWEGUNG_SPIELFIGUR-mal schnell durch, damit der Spieler
            ' zwischen zwei Hindernis-Vorschüben reagieren kann.
            For i As Integer = 1 To BEWEGUNG_SPIELFIGUR

                ' Tasteneingabe abfragen (nicht-blockierend)
                Dim taste As Integer = Tastatur_Abfrage()

                ' Spieler-Model löschen (alte Position)
                Loesche_Spieler(spielfigur_spalte)

                ' ---- Spieler bewegen ----
                If taste = CURSOR_LEFT Then
                    spielfigur_spalte -= 1
                    ' Linke Grenze: +1 wegen des linken Bauteils
                    If spielfigur_spalte < 1 Then spielfigur_spalte = 1
                End If
                If taste = CURSOR_RIGHT Then
                    spielfigur_spalte += 1
                    ' Rechte Grenze: -1 wegen des rechten Bauteils
                    If spielfigur_spalte > SPALTE_MAX - 1 Then spielfigur_spalte = SPALTE_MAX - 1
                End If

                ' ---- Schuss abfeuern (Leertaste) ----
                If taste = KEY_SPACE Then
                    If ammo > 0 Then
                        Dim abgefeuert As Boolean = Feuere_Schuss(schuesse, spielfigur_spalte)
                        If abgefeuert Then
                            ammo -= 1   ' Eine Munition verbrauchen
                        End If
                    End If
                End If

                ' ---- Schüsse aktualisieren (Bewegung + Kollision) ----
                Aktualisiere_Schuesse(schuesse, spielfeld, score)

                ' ---- Spieler-Model an neuer Position zeichnen ----
                Zeichne_Spieler(spielfigur_spalte)

                ' ---- Kollision: Spieler <-> Hindernis ----
                ' Geprüft wird Zeile 22 (direkt über der Spielerzeile)
                ' Treffer auf alle 3 Teile des Models prüfen
                Dim getroffen As Boolean = False
                If spielfeld(SPIELER_ZEILE - 2, spielfigur_spalte) = HINDERNIS_ZEICHEN Then getroffen = True
                If spielfigur_spalte - 1 >= 0 Then
                    If spielfeld(SPIELER_ZEILE - 2, spielfigur_spalte - 1) = HINDERNIS_ZEICHEN Then getroffen = True
                End If
                If spielfigur_spalte + 1 <= SPALTE_MAX Then
                    If spielfeld(SPIELER_ZEILE - 2, spielfigur_spalte + 1) = HINDERNIS_ZEICHEN Then getroffen = True
                End If

                If getroffen Then
                    ' Treffer! Leben abziehen, Hindernis löschen, Beep
                    leben -= 1
                    Console.Beep()

                    ' Die Hindernisse um den Spieler herum löschen
                    spielfeld(SPIELER_ZEILE - 2, spielfigur_spalte) = " "c
                    If spielfigur_spalte - 1 >= 0 Then spielfeld(SPIELER_ZEILE - 2, spielfigur_spalte - 1) = " "c
                    If spielfigur_spalte + 1 <= SPALTE_MAX Then spielfeld(SPIELER_ZEILE - 2, spielfigur_spalte + 1) = " "c
                End If

                ' ---- HUD aktualisieren ----
                Zeichne_HUD(leben, score, ammo)

                ' Kurze Pause pro Tick (Gesamtwartezeit aufgeteilt)
                Threading.Thread.Sleep(CInt(wartezeit / BEWEGUNG_SPIELFIGUR))
            Next   ' Ende Bewegungsschleife

            ' SCHRITT 6: Score pro überlebter Runde erhöhen
            score += 10

            ' SCHRITT 7: Tastaturpuffer leeren (verhindert Eingabe-Stau)
            Do
            Loop Until Tastatur_Abfrage() = NO_KEY

            ' SCHRITT 8: Schwierigkeit steigern
            wartezeit *= 0.99    ' Spielfeld bewegt sich schneller
            If wartezeit < 30 Then wartezeit = 30   ' Minimalgeschwindigkeit
            a_max *= 1.03        ' Mehr Hindernisse pro Zeile

        Loop Until leben <= 0   ' Schleife endet wenn alle Leben aufgebraucht

        ' ---- Spielende ----
        Game_Over(score)
        Pruefe_Highscore(score)
    End Sub

    '  ABSCHNITT 10: EINSTIEGSPUNKT

    ' SUB: Main
    ' Startet das Programm, zeigt das Hauptmenü
    ' und leitet zur gewählten Aktion weiter.
    Sub Main()
        Console.CursorVisible = False
        Console.Title = "DOOM – Console Edition"
        Console.BackgroundColor = ConsoleColor.Black
        Console.ForegroundColor = ConsoleColor.White

        Dim laeuft As Boolean = True

        Do While laeuft
            Dim auswahl As Integer = Zeige_Hauptmenue()

            Select Case auswahl
                Case 1
                    ' Einzelspieler starten
                    Console.Clear()
                    Spielablauf()

                Case 2
                    ' Hall of Doom (Highscores) anzeigen
                    Zeige_Highscores()

                Case 3
                    ' Spiel beenden
                    laeuft = False
            End Select
        Loop

        ' Abschlussbildschirm
        Console.Clear()
        Console.ForegroundColor = ConsoleColor.DarkRed
        Console.SetCursorPosition(22, 12)
        Console.WriteLine("Bis zum naechsten Mal, Marine... RIP AND TEAR!")
        Threading.Thread.Sleep(2000)
        Console.Clear()
    End Sub

End Module