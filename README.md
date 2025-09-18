# Blazor Control Center – Microclimate Dashboard

## Overview  
**Blazor Control Center** is a web-based dashboard built with **ASP.NET Core Blazor Server**. It is designed to connect to microcontroller-based clients (e.g., Arduino) equipped with **Sensirion SCD41** sensors for **CO₂, temperature, and humidity** monitoring.  

The system collects real-time sensor data via a TCP server, displays it on interactive **ApexCharts** dashboards, and will support event-driven automation (e.g., relay-controlled fans and humidifiers) based on configurable thresholds.  

Although developed for **mushroom cultivation**, this project can be repurposed for any grow room or controlled environment that requires precise climate monitoring and automated responses.

---

Features  
**TCP Server Integration**  
  - Listens for Arduino/microcontroller clients over TCP/IP.  
  - Handles multiple clients concurrently with unique IDs.  

**Sensor Data Handling**  
  - Supports **SCD41 sensor data**: CO₂ (ppm), Temperature (°F), Humidity (%).  
  - Parses and validates incoming data streams from clients.  
  - Converts temperature readings from Celsius to Fahrenheit automatically.  

**Blazor Web Dashboard**  
  - Interactive **ApexCharts** for real-time visualization of sensor data.  
  - Client connection logs and event messages available in the UI.  
  - Responsive layout with sidebar navigation.  

**Planned Features**  
  - **SQL Database Integration** for historical data storage.  
  - **Historical Events Tracking** (sensor events, relay triggers, thresholds).  
  - **Automation Support** – trigger relays to:  
    - Control fans when humidity is too high.  
    - Activate humidifiers when humidity is too low.  
    - Regulate temperature with heating/cooling systems.  
    - Adjust CO₂ levels based on ppm thresholds.
