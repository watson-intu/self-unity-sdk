# Intu Unity SDK
 
Intu is an architecture that enables Watson services in devices that perceive by vision, audition, olfaction, sonar, infrared energy, temperature, and vibration. Intu-enabled devices express themselves and interact with their environments, other devices, and people through speakers, actuators, gestures, scent emitters, lights, navigation, and more.

This SDK allows a Unity 3D application to connect to a running self instance and extend self with sensors, gestures, or agents implemented within unity.

## Before you begin

  * To use Intu, you must have a Bluemix account. To register for a Bluemix account, go to https://console.ng.bluemix.net/registration/. 
  * Ensure your hardware meets the following requirements:
  
    * Windows
    
      * Intel® Core 2 or AMD Athlon® 64 processor; 2 GHz or faster processor
      * Microsoft Windows 7 with Service Pack 1, Windows 8.1, or Windows 10, Windows 2013
      * 2 GB of RAM (8 GB recommended)
      * 1 GB of available hard-disk space for 32-bit installation; 1 GB of available hard-disk space for 64-bit installation; additional free space required during installation (cannot install on a volume that uses a case-sensitive file system)
      * 1024 x 768 display (1280x800 recommended) with 16-bit color and 512 MB of dedicated VRAM; 2 GB is recommended
      * OpenGL 2.0–capable system
      * CPU: SSE2 instruction set support
      * Graphics card: DX9 (shader model 3.0) or DX11 with feature level 9.3 capabilities.
      * Internet connection and registration are necessary for required software activation, validation of subscriptions, and access to online services.

    * Mac OS
    
      * Multicore Intel processor with 64-bit support
      * Mac OS 10.12, 10.11, 10.10, 10.9
      * 2 GB of RAM (8 GB recommended)
      * 1GB of available hard-disk space for installation; additional free space required during installation (cannot install on a volume that uses a case-sensitive file system)
      * 1024 x 768 display (1280x800 recommended) with 16-bit color and 512 MB of dedicated VRAM; 2 GB is recommended
      * OpenGL 2.0–capable system (This is must have for Unity Application)
      * CPU: SSE2 instruction set support
      * Graphics card: DX9 (shader model 3.0) or DX11 with feature level 9.3 capabilities.
      * Internet connection and registration are necessary for required software activation, membership validation, and access to online services.

## Getting started

Getting started includes the following tasks:

1. [Requesting access to the Watson Intu Gateway](#requesting-access-to-the-watson-intu-gateway)
2. [Downloading Intu](#downloading-intu)
3. [Installing Intu](#installing-intu)

### Requesting access to the Watson Intu Gateway

1. Request access to the Watson Intu Gateway. Open [Intu Gateway](https://rg-gateway.mybluemix.net/).
2. Click **Log In** and specify your IBM Bluemix credentials.
3. In the **Organization** field, specify the name of the organization that you represent.
4. In the **Business Justification** field, briefly explain why you need access to the Intu Gateway.
5. Click **Submit**. After your request for access is approved, you receive a confirmation email.
6. Open the confirmation email, and click the link. The Intu Gateway Log In page is displayed again.
7. Click **Log In**. The Intu Downloads page is displayed.

### Downloading Intu

1. On the Intu Downloads page, download the appropriate installation package for your platform.
2. Extract the files from the package into your working directory.

### Installing Intu

1. In your working directory, double-click Intu Manager. If a security warning is displayed, accept the risk and open the file.
2. Select the **Windowed** checkbox, and click **Play!**. The Intu Tooling page is displayed.
3. Click **Install Intu**, and a new Intu Tooling sign-in page is displayed.  
4. Click **Log In**. You are prompted to return to the Intu Manager application.
5. A page displays options for where you can choose to install Intu. For this workshop, select local machine, and click **Next**. A page displays your organization in the dropdown menu, and defaultGroup is selected.
6. Click **Install**. Installing Intu takes a few minutes. During the installation process, if you see one or more security prompts, allow access. After the installation process is complete, your instance of Intu is preconfigured with the following Watson services: Conversation, Natural Language Classifier, Speech to Text, and Text to Speech. The preconfiguration is enabled for a trial period of 24 hours. If you want to test Intu after the trial period, see [After DevCon ends](#after-devcon-ends).

After Intu is installed, the Intu Manager window is displayed, and you're prompted to select your group. Your organization and group should be preselected in the dropdown menu.

3. Click **Next**. A "Connecting to parent..." message is displayed while your Intu embodiment tries to establish a connection. During this part of the process, the box beside your embodiment is red and labeled with Off. After the connection is established, the box is green and labeled On.
4. Doubleclick your embodiment. The Menu option is displayed.

## Configuring Intu
Your installation is preconfigured to use the Conversation, Natural Language Classifier, Speech to Text, and Text to Speech services. To configure Intu to use your instances of these services, complete the following steps.

**Pro tip:** As you complete this task, you'll receive credentials for each service instance, and you'll need these credentials later. Open a new file in your favorite text editor and create a section for each service so that you can temporarily store its credentials.

1. [Log in to Bluemix](http://www.ibm.com/cloud-computing/bluemix/).
2. On the Bluemix dashboard, click **Catalog** in the navigation bar.
3. Click **Watson** in the categories menu.
4. Create an instance of the Conversation service.
  1. Click the **Conversation** tile.
  2. Keep all the default values, and click **Create**.
  3. Click the **Service Credentials** tab.
  4. Click **View Credentials** for the new service instance.
  5. Copy the values of the `password` and `username` parameters and paste them in your text file.
  6. Click the **Watson** breadcrumb.
  7. Add the next service instance by clicking the hexagonal **+** button.
5. Create instances of the Natural Language Classifier, Speech to Text, and Text to Speech services by repeating the same steps 1 - 7 that you completed to create the Conversation service instance.
6. Specify your service credentials in Intu Gateway.
  1. Expand **All Organizations** by clicking the arrow icon.
  2. Click the name of your organization.
  3. Expand your organization by clicking the arrow icon.
  4. Click the name of your group.
  5. Click **Services** in the navigation bar.
  6. For your instances of the Conversation, Natural Language Classifier, Speech to Text, and Text to Speech services, click **Edit**, specify the user ID and password, and click **Save**.

**Important:** Do not change the service endpoint unless you are an enterprise user.

### Installing the Intu Starter Kit

The Intu Starter Kit contains a Conversation service workspace that helps you visualize how intents, entities, and dialog are developed. You can expand on the workspace in the kit or use it as a guide for developing your own later.

1. Log in to Intu Gateway.
2. Click **Downloads**.
3. Download the Intu Starter Kit.
4. Complete the instructions in `readme.txt`.

## Using Intu for Unity

### Getting the files

Download the code to your computer. You can do download the code in either of the following ways:

  * This project depends on the Watson Developer Cloud Unity SDK, you will need to install that into your Unity project before installing this project. See https://github.com/watson-developer-cloud/unity-sdk
  * Download the .zip file of this repository to a local directory.
  * Clone this repository locally.


### Release Notes
  * None

### Installing Files

1. Copy the files from the zip or git clone into your Unity project Assets/ folder (typically Assets/SelfUnitySDK)

## Feedback

Post your comments and questions and include the `self` tag on 
[dW Answers](https://developer.ibm.com/answers/questions/ask/?topics=watson)
or [Stack Overflow](http://stackoverflow.com/questions/ask?tags=ibm-watson).
