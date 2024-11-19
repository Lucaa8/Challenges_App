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

## Disclamer
To fully understand the concepts covered in the application, it would be helpful for the reader to have a basic understanding of how challenges work. These concepts will not be re-explained here; only how the plugin is modified from the C# interface will be discussed. For more details about the plugin's functionality, please refer to the [plugin README](https://github.com/Lucaa8/Challenges_Plugin/blob/main/README.md).

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
The creation of items is arguably the most complex part of the application, and it will not be possible to demonstrate everything. However, every effort will be made to help the reader understand the importance and usefulness of this section. In Minecraft, creating complex items manually can be very time-consuming and tedious, even more so in JSON configuration files. For this reason, the application includes several complete pages to quickly create specific and complex items and assign them directly to challenges. 

Items are used in multiple contexts:
- As the icon for a category or challenge.
- As prerequisites for completing an "inventory" challenge.
- As rewards for a challenge.

For this reason, throughout the illustrations, you will see dropdown lists that allow users to select items. When a user loads a category or a challenge, all items associated with that element are fetched from the server and added locally. This way, the user can modify or delete them if needed. 

The small blue spinning button next to the dropdown lists is a refresh button that updates the list with all newly created items (locally or fetched from the server).

| ![item_create](https://github.com/user-attachments/assets/41f1ccae-2bd7-4f97-bf14-11e2e02720f6) | 
|:--:| 
| *Create a new local item (the name you put here will appear in drop-down lists later in the app)* | 

| ![item_material](https://github.com/user-attachments/assets/dd61b01d-ca84-44c7-9307-0274f90721fa) |
|:--:| 
| *No need to learn by heart the 1000 Minecraft material list, this little window will help you!* | 

| ![item_enchants](https://github.com/user-attachments/assets/e2195abe-5dcc-4642-92c2-5a5879a64252) |
|:--:| 
| *Keep track of your item's enchantments and edit them in few clicks* | 

| ![item_flags](https://github.com/user-attachments/assets/6dd52b63-4ef6-4732-9358-8e39002f814a) |
|:--:| 
| *Just toggle all the existings Minecraft flags on your item by double clicking this list!* | 

| ![item_attributes](https://github.com/user-attachments/assets/d7d28bae-93de-4fe6-a4ee-72ddf17c0c49) |
|:--:| 
| *Add infinite attributes on your item and edit them in 2 clicks, who told you attributes are boring to create?* | 

| ![item_meta_1](https://github.com/user-attachments/assets/10d0e7a7-46ce-4baf-b975-c874bb2f9cf4) | ![item_meta_2](https://github.com/user-attachments/assets/3be9bd50-7db2-42b7-8563-540fbee74007) |
|:--:|:--:| 
| *Preview your Leather armor's color just by looking the app (Yeah the item is named potion... my bad)* | *Write a really nice book and preview the Minecraft result by toggling one checkbox, do you like Harry Potter?* |

| ![item_add](https://github.com/user-attachments/assets/1353e1bc-7de5-4aec-a45e-f1e21371fc8b) | ![acacia_book](https://github.com/user-attachments/assets/4f343b46-dc47-4449-b409-da78a82b3ac9) |
|:--:|:--:| 
| *Finally! Just add your beautiful item in any challenge just by opening a dropdown-list* | *Here is the in game result* |

### Categories
A category contains the following information:
- **UUID**: All JSON content is stored in a file named after the UUID of the category followed by `.json`. The UUID is chosen randomly during creation and cannot be changed.
- **Name**: Can include spaces and special characters, as it is not used as an identifier.
- **Description**: Similar to the name but supports color codes for styling.
- **Active Status**: Indicates whether the category is active or not. Inactive categories are still visible in-game but appear grayed out.
- **Glass Pane Color**: Specifies the color of the glass pane surrounding the inventory when viewing challenges.
- **Icon**: An item created in the app, displayed in-game as the "logo" of the category.
- **Icon Data**: Previously stored separately, now embedded directly in the item (deprecated).
- **Page and Slot**: Defines where the category icon will appear within the main menu inventory.
- **List of Required Challenges**: A list of challenges that must be completed to unlock this category. This should not be confused with the list of challenges hosted within the category, which is defined elsewhere.

| ![category_page](https://github.com/user-attachments/assets/07e894d8-62ea-4ec4-9cb6-dc057814c82d) | ![cat_demo](https://github.com/user-attachments/assets/074c3888-337b-49ec-a404-ba1fb3c8aca2) |
|:--:|:--:| 
| *An example category page* | *In game result* |

The "Envoyer" (Send) button is used to push the category to the server after modifications are made. The change is made instantly and everyone can see the result in game, without restarting the server.

### Challenges
A challenge contains the following information:
- **UUID**: All JSON content is stored in a file named after the UUID of the challenge followed by `.json`. The UUID is chosen randomly during creation and cannot be changed.
- **Category**: Here is defined which category will host this challenge.
- **Name**: Can include spaces and special characters, as it is not used as an identifier.
- **Description**: Similar to the name but supports color codes for styling.
- **Type**: Items (Inventory), Island, Statistics and Others, which will decides what the challenge will ask as requirements. Check the Requirements section
- **Active Status**: Indicates whether the challenge is active or not. Inactive challenges are still visible in-game but appear grayed out.
- **Icon**: An item created in the app, displayed in-game as the "logo" of the category.
- **Page and Slot**: Defines where the category icon will appear within the main menu inventory.
- **Limit**: How many time this challenge can be completed at total (not daily) by an island (not by player on island)
- **List of Required Challenges**: A list of challenges that must be completed to unlock this challenge.
- **Requirements**: What does the challenge needs to be considered as completed
- **Rewards**: First and Next rewards. Including Items, Commands, Message, Money and Experience

| ![challenge_page](https://github.com/user-attachments/assets/b9c128e3-6dd3-4f56-912c-3615ad220e53) | ![cha_demo](https://github.com/user-attachments/assets/c9d3c09e-7c9d-41ef-8f5c-358e7ee8692e) |
|:--:|:--:| 
| *An example challenge page* | *In game result* |

#### Requirements
This section will showcase the configuration of each prerequisite in a simple and quick manner, with illustrations of the application and its in-game rendering.

##### Inventory (or Items) Requirements
| ![req_inv_add](https://github.com/user-attachments/assets/b9d523d5-e3cf-47c3-b566-0fae7c97b466) | ![challenge_req_inv_proof](https://github.com/user-attachments/assets/dea281fd-5e39-46f1-b123-667ed5bcdfbb) |
|:--:|:--:| 
| *Adding a pickaxe efficiency 5 to the required items of the challenge* | *In game result* |

##### Island (Blocks and/or Entities) Requirements
| ![req_is_add](https://github.com/user-attachments/assets/50c94b37-331d-464e-9c61-b2e94455adee) | ![challenge_req_is_proof](https://github.com/user-attachments/assets/6d0791a1-319b-4154-91d2-f27d28411272) |
|:--:|:--:| 
| *Adding entities and blocks to the requirements of the challenge. Only need 2 clicks by element added.* | *In game result* |

##### Statisticals Requirements
| ![req_stat_add](https://github.com/user-attachments/assets/47cf3b72-e8e4-46e5-9184-292c569b7a06) | ![challenge_req_stat_proof](https://github.com/user-attachments/assets/d1c276d5-02cc-462a-938f-33d4f9289f64) |
|:--:|:--:| 
| *Adding and editing statistics with substatistics to the requirements of the challenge.* | *In game result* |

##### Others (Money, Experience and Island Levels) Requirements
| ![req_others_add](https://github.com/user-attachments/assets/db2fa3d5-b45a-4079-9ddc-a265fc212e9a) | ![challenge_req_others_proof](https://github.com/user-attachments/assets/87b538b7-ca27-49a5-a297-df05596b9b74) |
|:--:|:--:| 
| *Adding money, experience and island levels to the requirements of the challenge.* | *In game result* |

#### Rewards
For the rewards section, we will review key elements such as the concepts of First and Next Rewards and items. Messages and commands will be set aside, as they are not particularly interesting aspects and are fairly basic. Below, you will find an illustration of creating a reward for one of our demonstration challenges, followed by its in-game rendering. 

| ![rew_add](https://github.com/user-attachments/assets/a71aeb77-bca0-470b-8ddd-0660150af5cc) |
|:--:| 
| *Adding some already created items to the reward pool of our first demo challenge. Setting money and experience too. For the first completion reward and the next ones* | 

| ![challenge_rew_proof](https://github.com/user-attachments/assets/73c6fc56-c117-4826-900e-60dc44a776f8) |
|:--:| 
| *Our challenge is finally ready. How beautiful ? We even got the 50% sword drop !* | 

## Conclusion

