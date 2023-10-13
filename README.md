# XR Orbit Selection
Selection of objects in XR by swiping, imitating orbiting circles. Unity project (swiping done in wearOS Android app).

This is a project developed for Oculus, to be used with an accompanying [wearOS app](https://github.com/SRSAS/OrbitSelectionWearApp). In it, objects are displayed in line, each with a target above it. The target is composed of two circles, one big and hollow, the other small and filled and orbiting the circumference of the large one. Each of the objects is supposed to be differentiated by its target having either a different orbiting speed or position.

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


## Adding targets and changing parameters
[^1]:To open the project, open Unity Hub, click on **Open**, then select the repository directory
[^2]:For troubleshooting connecting the Meta Quest Pro to your computer, see the [official Meta documentation](https://developer.oculus.com/documentation/unity/unity-env-device-setup/)
