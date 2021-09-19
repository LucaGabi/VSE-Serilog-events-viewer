# VSE Serilog events viewer
Visual Studio Code Extention to inspect serilog events

This vscode extention allows inspection of logs created with serilog sinks here: https://github.com/serilog/serilog-sinks-mongodb

Features:
- filter by time frame, level, content
- filter by expresion (similar to where clause in SQL)
- persist config file for future inspection
- shortcuts for fast use:
    - 'f' toggle filter panel
    - 'esc' toggle expression filtering
    - inside expression editor 'ctrl-up' & 'ctrl-down' sets previous or next expression in current session history
    - 'a' move time-frame back
    - 'x' move time-frame forward    
    - 'e' filter by level error
    - 'w' filter by level warning
    - 'd' filter by level debug
    - 'i' filter by level info
    - 'q' filter by level info

TIP: ssh would be a good use to provide the connection in a secure manner

Create new connection

![](https://github.com/LucaGabi/VSE-Serilog-events-viewer/blob/main/l.c.gif)

Open from existing config

![](https://github.com/LucaGabi/VSE-Serilog-events-viewer/blob/main/l.o.gif)

