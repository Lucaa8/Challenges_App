# Challenges-App
This application, fully coded from scratch in .NET Core 6.0, was designed to assist in the configuration of the Minecraft plugin "[Challenges](https://github.com/Lucaa8/Challenges_Plugin)," which was also created by me. The plugin is fully configured using JSON. It is highly advanced, allowing the creation of an infinite number of objects and challenges organized into different categories. However, with hundreds of lines of JSON, human error can happen quickly. That’s why a GUI application was developed (in C#) in addition to the plugin (in Java) to simplify editing and better visualize challenges when staff members need to make changes. The staff member starts an editing session from the Minecraft server and can then log into the C# application using a temporary token. The application connects to the Minecraft server via a socket, and any changes are made live.

## ⚠️ Usage Restrictions
This code is provided for **exposition and demonstration only**. You are not allowed to:
- Download a compiled release of this project for public use.
- Compile or use **any** of this code, whether for commercial or non-commercial purposes.
### Important:
Any use of this code in violation of these restrictions, especially on publicly accessible servers or for paid services, constitutes a breach of the terms. If you wish to discuss a specific use case, please contact me directly.

## Dependencies
- [DOTNET Core 6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)

## Design and conception
The idea behind this application and every design concept is from me.

## Implementation
I handled the implementation myself, based on what I planned during the conception. However, the application has been extensively used from start to finish by [BlackT8](https://github.com/BlackT8), which greatly facilitated the testing and bug-fixing stages.

## Functionalities
To summarize, the application allows for modifying the current status of challenges (active/inactive), viewing and loading any existing category or challenge on the server. It can create, modify, or delete very complex items (Enchantments, Attributes, Meta like Potions, Books, etc.). Additionally, it allows for creating a completely new challenge/category, modifying it, and deleting it. It is even possible to download the final JSON content of an item or import a challenge or category using this JSON content. All changes can be pushed directly to the server without reloading/restarting, which helps avoid excessive maintenance periods and allows for immediate in-game visualization of changes when modifications are made by staffs.

### How to Connect/Disconnect
Some general information about the communication protocol between the Minecraft server and the application: only one session can be active at a time to avoid conflicts when modifying multiple categories or challenges simultaneously. I’m not going to create a Github2 to synchronize staff work, lol. The application uses a standard TCP socket connection between the client (C#) and the server (Java). Users authenticate with an access token generated by an admin on the server. All packets sent are logged both on the client and the server, making it easier to track and visualize the communication process.

#### Connection
When the application is launched, a form asking for an access token and an IP address will be visible. The IP address is the address of the Minecraft server (whether remote or local), and a port must be specified. This is the port on which the Minecraft server listens for connections, and it is specified in the plugin's configuration. Finally, the access token can be obtained from the game. Players with a certain permission can execute the command `/cadmin editor new` to ask the server to start listening on the defined port and receive a freshly generated access token.

| ![editor_new_mc](https://github.com/user-attachments/assets/25c71d0a-4133-46d3-a071-92130d827598) | ![editor_new_app](https://github.com/user-attachments/assets/eecf1c0d-c34b-44fa-8868-34e229d4458d) |
|:--:|:--:| 
| *An admin execute this command on the Minecraft server and copy the token* | *The admin goes to the app and paste the token with the server's IP and port* |

**Important** The staff who generated the token must be the same staff member who enters the session in the application. When connecting with the access token, if the IP address does not match between the player in-game and the IP address of the login packet received from the application on the server, the server will immediately terminate the session, and an error message will warn the user on the application. This is a security measure to ensure that staff do not give access to sessions to players who should not have access to the application, or simply to prevent the access token from being intercepted by someone else.

#### Disconnection
There are two ways to disconnect. The first is simply by closing the application using the close button (the cross). The second way is from the game or the server console. Any staff member can execute the command `/cadmin editor kill [optional reason]`. This will terminate the current session and notify the connected person if a reason was provided.

It is important to note that if the application crashes unexpectedly or if the user presses ALT+F4, the server will terminate the session to avoid wasting resources and preventing the session from being stuck indefinitely. To achieve this, a keep-alive mechanism is in place: after 1 minute without receiving any information from the client, the server will close the session.

| ![editor_kill_mc](https://github.com/user-attachments/assets/c77ec371-0e36-4f56-b21a-effe724e0d60) | ![editor_killl_app](https://github.com/user-attachments/assets/df004442-cb31-4e72-840a-66b22884c078) |
|:--:|:--:| 
| *An admin execute this command on the Minecraft server* | *The connected user is disconnected and see the reason specified* |

### Main Page
When an aggregated user connects, the first thing the client will do is fetch the current state of the challenges (active, inactive?) and then retrieve the list of names of existing categories and challenges. Nothing more. It would be too time-consuming to load each category, challenge, and item individually without being sure that the user wants to modify them. The application will then update this information on the graphical interface (which is non-blocking during transfers with the server).

| ![Challenges Editor 19_11_2024 00_35_51](https://github.com/user-attachments/assets/d5377ed6-148f-479f-8312-d8319132c8bd) |
|:--:| 
| *Menu page* |

On this page, it is possible to view:
- The current state of the challenges and modify it directly from the application (In case the staff no longer wants players to use the challenges while making major modifications).
- Filter categories or challenges by their name
- A button to access item management.
- The connection status with the server (ping and time of the last packet exchange).
- A panel displaying various actions the user performs within the application and incoming/outgoing packets with the server.
- Finally, the list of categories and challenges currently present in the plugin.

The user can then click on an existing category or challenge, which will load the relevant information about that category or challenge from the server (such as its description, whether it is active, its icon, etc.), and display it on a dedicated page for that category or challenge. (For a detailed description of these pages, see the next sections, namely **Categories** and **Challenges**). The user can also click on "New Category" or "New Challenge," which will create a **local** new element. They can then modify it and send it to the server. Finally, it is possible to delete an item by right-clicking on its name.

| ![editor_create_challenge](https://github.com/user-attachments/assets/f95847f8-ddd9-4740-9455-f951dd21693f) |
|:--:| 
| *Create a challenge by double clicking on nouveau challenge, then you can find it in the list and edit it* |

| ![editor_delete_challenge](https://github.com/user-attachments/assets/665ee88d-893d-4b16-982f-f806f8984d43) | ![editor_del_popup](https://github.com/user-attachments/assets/2ab19673-b36a-4bab-a625-b5e9c235eb61) |
|:--:|:--:| 
| *Delete an element by right clicking it (the confirmation popup did not show on the record)* | *The confirmation popup* |


| ![editor_del_challenge_fail](https://github.com/user-attachments/assets/5a6422f9-34ec-4b2c-bee3-dac4ff854490) |
|:--:| 
| *Cannot delete a non empty category* |

### Items Creation

### Categories

### Challenges
