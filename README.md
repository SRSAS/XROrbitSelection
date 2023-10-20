# XR Orbit Selection
Selection of objects in XR by swiping, imitating orbiting circles. Unity project (swiping done in wearOS Android app).

This is a project developed for Oculus, to be used with an accompanying [wearOS app](https://github.com/SRSAS/OrbitSelectionWearApp).

Objects are displayed in line, each with a target above it. The target is composed of two circles, one big and hollow, the other small and filled and orbiting the circumference of the large one. Each of the objects is supposed to be differentiated by its target having either a different orbiting speed or position.

To select an object, the user, wearing the headset and a smartwatch running the accompanying wearOS app, must perform a swipe on the smartwatch that matches the orbit of the target of the object that they wish to select. While the user keeps performing the swipe, the object that most closely matches their swipe will be highlighted. If it isn't the one the user intended, they can keep performing the swipe to try to match with the correct object. Once the user lifts their finger from the smartwatch, the currently highlighted object will be selected and be highlighted with a different colour.

Matching of the user's swipe with the targets' orbit is done through calculating a Pearson's correlation of the X coordinates and of the Y coordinates between the swipe and the orbits of each target. This correlation is calculated periodically with the data gathered in that period, and only in that period. i.e. after a correlation is calculated, all data on the user's swipe and the targets' orbits is dumped, and we start collecting new data from that moment until the next correlation calculation.

There is also a threshold of correlation that must be passed before any object is highlighted. When the user starts swiping, no object is highlighted. Only once the user's swipe matches a target past the threshold will it be highlighted. Afterwards, there will always be a highlighted target until the user finishes the swipe. If after an object has been highlighted there is a correlation calculation that doesn't pass the threshold for any target, the last highlighted target will remain highlighted.

This is meant as subtle and low cost method of selection for XR.

## Authorship
Project authored by **Sebastião Andrade e Sousa**

[LinkedIn](https://www.linkedin.com/in/sebasti%C3%A3o-andrade-e-sousa-700827270/) -- [GitHub](https://github.com/SRSAS)


Developed during an internship researching subtle interactions with mobile XR for [HCI Lab @ IST](https://web.tecnico.ulisboa.pt/augusto.esteves/)

Project supervised by Professor [Augusto Esteves](http://web.tecnico.ulisboa.pt/augusto.esteves/EstevesCV-September2023.pdf)

All rights belong to the HCI Lab.

## Project Structure
### SDKs
This project uses Meta's [Movement SDK for Unity](https://developer.oculus.com/documentation/unity/move-overview/).
### Scripts
|Script|Description|
|----|-----------|
|**BillBoarding**|Makes this gameObject always face towards the camera.|
|**Selectable**|Holding this script is what makes an object selectable. It adds a **Target** to the object on _Start()_. On _Update()_ it controls the **Target**'s orbit, and also displays the object's selection state. The **Selectable** object is also responsible for calculation of the correlation of its **Target**'s coordinates with the user swipe. A **Selectable** object listens to _UnityEvents_ from the **SelectionManager** for when to take in data from the user, and when to calculate correlations. This script also provides an interface for being hovered and selected.|
|**TargetManager**    |This script is an aggregator of **Selectable** objects, and also serves as a connection between them and the **SelectionManager**. On _Start()_ turns all of the object's children into **Selectable** objects, displays them in a line, sets their positions and speeds as explained [further](https://github.com/SRSAS/XROrbitSelection/edit/main/README.md#customizing-targets-as-a-group), and makes them listen for the **SelectionManager** _UnityEvents_. On _Update()_ checks if there are any new children and adds them as **Selectable** objects, like the ones on _Start()_. And, if any of the [target motion parameters](https://github.com/SRSAS/XROrbitSelection/edit/main/README.md#customizing-targets-as-a-group) have changed, updates the targets' motions. The **TargetManager** also provides an interface to hover the target with the highest correlation, once these have been calculated, or to select a target, once a user has stopped swiping.|
|**SelectionManager**|This script controls the selection parameters (time and threshold). It is the connection between the **targetManager** and the **SocketManagerThreading**. It takes the input from the user, from the **SocketManagerThreading** object, and through a _UnityEvent_ passes the user input coordinates to the **Selectable** objects. When it is time to calculate the correlation with the **Targets**, it invokes another _UnityEvent_, and tells the **TargetManager** to hover the **Selectable** with the highest correlation. When the user stops their swipe, it tells the **TargetManager** to select the last hovered **Selectable**. Also sends information to **TestManager**.|
|**SocketManagerThreading**| C# native socket and threading used to receive user input. Can use either UDP or TCP by changing the protocol field **_before_** running. **_UDP is recommended_** for a faster and simpler connection.|
|**TestManager**|Connects to **TargetManager** and **SelectionManager**, and listens to **SelectionManager** _UnityEvents_ to gather information about the user's selection process, and prints a report with that information to the console.|
### Prefabs
|Name|Scripts|Description|
|----|-------|-----------|
|**Target**|_BillBoarding_|Large hollow circle, with smaller filled circle that orbits the larger circle's cirumfrence. Always faces the camera. It's what the user must imitate with their swipe.|
|**Selector**|_SocketManagerThreading_, _SelectionManager_, _TargetManager_| This object manages the whole selection process. For the scripts to function properly, they should be placed in the same gameObject. This is the object that will also hold all of the **Selectable** objects.|
### SampleScene
- OVRCameraRig:
    - This is the camera for the Meta headset. It is a prefab taken directly from the Meta movement SDK. For more information, please refer to Meta's [documentation](https://developer.oculus.com/documentation/unity/unity-tutorial-hello-vr/).
- Selector.
## Cloning and Setup

1.  Clone the repository;
2.  Open the project on Unity Hub[^1];
3.  On the Unity Editor go to **File** > **Build Settings...** and click on **Android**;
4.  Click on **Switch Platform** (lower right corner);
5.  Then on that same window, in the lower left corner click on **Player Settings...**;
6.  On the column on the left select **Oculus**;
7.  On that page, check each tab's check list for outstanding issues and press **Fix All** if there are any;
8.  On the Project panel, go to Assets > Scenes and place the SampleScene onto the Hierarchy panel;
9.  Finally, delete the unnamed scene that was in the Hierarchy by default.

## Deploying on the Meta Quest Pro
If you encounter any problem deploying on the Meta Quest Pro, please follow the [official Meta documentation](https://developer.oculus.com/documentation/unity/unity-tutorial-hello-vr/).

1.  Connect the **Meta Quest Pro** via USB to your computer [^2];
2.  on the Unity Editor go to **File** > **Build Settings...**;
3.  Focus on the **Run Device** list and select the **Meta Quest Pro**;
4.  Finally, click on **Build and Run** to run the scene on your headset.




## Pairing with the App
This project has the option of using either _UDP_ or _TCP_ protocol to interact with the app.
To choose a protocol, on the **Unity Editor** > **Hierarchy Panel** select the **Selector** game object. Then, on the **Inspector Panel** expand the **Socket Manager Threading** script, and on the **Protocol** field select either _UDP_ or _TCP_.

**_IMPORTANT: The device running the wearOS app must be connected to the same network as the headset_**

### Using UDP
On the wearOS app simply input the headset's _IPV4_ and click on **Connect**.
You can start sending user input.

### Using TCP
On the wearOS app input the headset's _IPV4_ and click on **Connect**. Check if the app has connected and advanced to the next page, if not, click on **Connect** again until it does.
When it does, you can start sending user input.


## Adding and customizing targets
### Adding targets
To add a target, simply add whatever object you want to be selectable to the scene as a child object of the **Selector** object. You can also create new game objects directly as child objects of the **Selector** game object.
All children of the **Selector** object will be spaced equally apart along a line, will have a target above them, and will be selectable.
### Target customization
It is possible to alter the speeds and positions of the targets' orbits. This can be done for all targets as a group, or for each target individually.
#### Customizing targets individually
To change a single target's speed and position, while the scene is running, on the **Hierarchy Panel** select the object you want to change, on the **Inspector Panel** expand the **Selectable** script and customize the **Angular Speed** and **Current Angle** fields.
#### Customizing targets as a group
To change the targets' speeds and positions as a group,  on the **Hierarchy Panel** select the **Selector** object, on the **Inspector Panel** expand the **Target Manager** script and customize the **Angular Offset**, **Target Speed** **Speed Offset**, and **Speed Multiplier** fields.

As a group, targets are managed by the Target Manager which holds all targets in a list. Thus, each target has their **index** in that list.

In the group, each target's initial angle is calculated as:

$target_{initial Angle} = target_{index}\times Angle Offset$

And each target's speed is calculated as:[^3]

$target_{speed} = (Target Speed + Speed Offset \times target_{index}) \times Speed Multiplier^{target_{index}}$

**_Field default values:_**
|Field               | Default Value             |
|--------------------|---------------------------|
|**Angular Offset**  |-1 == ($2\pi/target Count$)|
|**Target Speed**    |1                          |
|**Speed Offset**    |0                          |
|**Speed Multiplier**|1                          |

## Getting test data
To get data from the user attempts to select objects, on the **Project Panel**, go to **Assets** > **Scripts**, and add the **TestManager** script to the **Selector** object on the **Hierarchy Panel**. Add a **Text - TextMeshPro** object to the scene (place it wherever you want on the screen), and place it in the **text** field of the **TestManager** script.

A random object will be picked and the text will tell the user (by the object's name) to select it. After the user's swipe, a report on the swipe will be printed to the console. This report includes information on swipe time, number of targets hovered over, etc.

This report is only printed to the console. To alter the report, or to send it to any other output stream, changes to the TestManager script must be made.

[^1]:To open the project, open Unity Hub, click on **Open**, then select the repository directory
[^2]:For troubleshooting connecting the Meta Quest Pro to your computer, see the [official Meta documentation](https://developer.oculus.com/documentation/unity/unity-env-device-setup/)
[^3]:Although this may seem overly complicated, this formula allows for easy differentiation of targets through the parameters. For example, by setting the Speed Multiplier to -1, half the target will orbit in one direction and the other half will orbit the other
