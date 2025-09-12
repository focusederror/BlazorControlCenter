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


---

## ⚙️ How It Works  
1. **TCP Server** (`TcpServerService`) runs as a background service.  
   - Accepts client connections and assigns them IDs.  
   - Routes sensor data to `ServerStateService`.  

2. **State Management** (`ServerStateService`) handles:  
   - Tracking active clients.  
   - Logging system events.  
   - Parsing incoming SCD41 data (CO₂, Temperature, Humidity).  
   - Broadcasting updates via events to the Blazor UI.  

3. **Web Dashboard**  
   - Displays live sensor data in **charts**.  
   - Provides logs for connection and event history.  
   - Serves as the control panel for future automation.  

---

## Getting Started  

### Prerequisites  
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)  
- Arduino or compatible microcontroller with **SCD40/SCD41 sensor**.  

### Running the Server  
```bash
# Clone the repository
git clone https://github.com/yourusername/BlazorControlCenter.git
cd BlazorControlCenter

# Run the Blazor server
dotnet run

