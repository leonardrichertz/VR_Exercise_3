VR Übung 3 - Tracking

Es wurde ein (HMD) Tracking-System implementiert, welches Daten von Trägheitssensoren (IMU) mit optischem Marker-basiertem Kamera-Tracking kombiniert.

Die APK sowie der Code befinden sich im Main-Branch.

Beide Ausgabeteile wurden in der Szene „Aufgabe_A” gelöst.

Überblick

Dieses Projekt demonstriert Sensor-Fusion-Techniken, die häufig in VR-Systemen verwendet werden, bei denen mehrere Tracking-Technologien kombiniert werden, um präzises Tracking zu gewährleisten (auch wenn einzelne Sensoren Einschränkungen haben).

In der interaktiven Umgebung befinden sich 4 Wände in unterschiedlichen Höhen und Lagen, hinter denen man sich vor den Kameras verstecken kann. 

Hauptfunktionen

- IMU-Simulation: Beschleunigungs- und Gyroskop-Simulation mit Rauschen und Drift
- Optisches Tracking: Kamerabasierte Tracking-Systeme mit Sichtfeld- und Sichtlinien-Validierung
- Sensor-Fusion: Kombination von IMU- und optischen Daten für Tracking
(- Echtzeit-Visualisierung: Live-Anzeige von Tracking-Daten und Systemstatus) - Die Datenvisualisierung als Art Head-Up Display wurde aus Usability Gründen ausgenommen.


Komponenten

1. IMU.cs - Simuliert Trägheitsmesseinheit-Sensoren
2. OpticalMarkerBasedCamera.cs - Implementiert optische Tracking-Kameras
3. SensorFusionTracker.cs - Kombiniert Daten aus mehreren Quellen
4. MathsHelpers.cs - Hilfsfunktionen für geometrische Berechnungen


IMU (Inertial Measurement Unit / Trägheitsmesseinheit)

Simuliert IMU-Verhalten einschließlich Sensorrauschen, Drift und Filterung.

1. Wichtige Eigenschaften
- LinearAcceleration: Gefilterte lineare Beschleunigung (m/s²)
- Velocity: Aktuelle Geschwindigkeit (m/s)
- AngularVelocity: Winkelgeschwindigkeit (Grad/s)
- Orientation: Aktuelle Orientierungs-Quaternion

2. Konfigurierbare Parameter
- accelerometerNoise: Simuliertes Beschleunigungsmesser-Rauschen
- gyroNoise: Simuliertes Gyroskop-Rauschen
- lowPassFilterFactor: Glättungsfilter-Stärke (0-1)
- driftCorrectionStrength: Drift-Korrektur-Rate (0-1)

3. Funktionen
- Tiefpassfilterung zur Rauschreduzierung
- Schwerkraft-Kompensation
- Automatische Drift-Korrektur
- Echtzeit-UI-Anzeige


OpticalMarkerBasedCamera

Simuliert optische Tracking-Kameras mit realistischen Einschränkungen.

1. Wichtige Eigenschaften
- AnglestoHMD: Horizontale und vertikale Winkel zum verfolgten Objekt
- HasValidTracking: Ob das Ziel aktuell verfolgbar ist
- Position: Kamera-Weltposition
- Forward: Kamera-Vorwärtsrichtung

2. Konfigurierbare Parameter
- horizontalFOV: Horizontales Sichtfeld (Grad)
- verticalFOV: Vertikales Sichtfeld (Grad)
- maxTrackingDistance: Maximale zuverlässige Tracking-Entfernung
- obstructionMask: Layer, die das Tracking blockieren können

3. (Tracking-Zustände) - ausgenommen
- Grün: Gültiges Tracking
- Rot: Kein Tracking (verdeckt oder nicht sichtbar)
- Gelb: Außerhalb der Reichweite
- Grau: Außerhalb des Sichtfelds


SensorFusionTracker

Haupt-Tracking-System, das IMU- und optische Daten kombiniert.

1. Wichtige Eigenschaften
- EstimatedPosition: Endgültige geschätzte HMD-Position
- EstimatedRotation: Endgültige geschätzte HMD-Rotation

2. Tracking-Logik
- Optisches Tracking verfügbar (2 Kameras): Verwendet Triangulation für Position
- Optisches Tracking verloren: Wechselt zu IMU-Integration
- Audio-Feedback: Spielt Sound ab, wenn Tracking verloren geht

3. Triangulations-Methode
- Verwendet mehrere Kamera-Strahlen zur 3D-Positionsschätzung
- Mittelt Schnittpunkte aller Kamera-Paare
- Behandelt parallele/nahezu parallele Strahlen elegant


MathsHelpers - wird nicht mehr für die Simulation verwendet

Hilfsklasse mit geometrischen Berechnungsfunktionen.

Funktionen
- GetVectorAtAngle(): Konvertiert Winkel zu Richtungsvektor
- GetLineIntersection(): Findet Schnittpunkt zweier 3D-Linien
