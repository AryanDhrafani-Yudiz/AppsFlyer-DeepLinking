# README: Integrating DeepLinking with AppsFlyer in Unity

## Overview
This guide explains how to integrate AppsFlyer DeepLinking functionality into your Unity project for Android and iOS platforms. It covers plugin setup, project configuration, and debugging steps to ensure seamless deep linking capabilities.

---

## Prerequisites
1. **Unity Editor**: Ensure Unity Editor is installed and configured.
2. **AppsFlyer Account**: Sign up and log in to AppsFlyer.
3. **Dependencies**: Download the required plugins:
   - **AppsFlyer Unity Plugin v6.15.3**
   - **External Dependency Manager v1.2.183**

   [Download Integration Folder](https://drive.google.com/drive/folders/1tO0d7vb8MgxGgee9BdZCUB5kT48wGGe2?usp=sharing)

---

## Step-by-Step Integration

### Step 1: Configure Build Settings
1. Navigate to **File > Build Settings**.
2. Switch the platform to **Android** or **iOS**.

### Step 2: Update Package Name and API Levels
1. Set the package name under **Edit > Project Settings > Player**.
   - Example package name: `com.aryan.deeplinkingtest`.
2. Configure **Minimum API Level** and **Target API Level** to meet AppsFlyer requirements.

### Step 3: Enable Custom Build Templates
1. Go to **Edit > Project Settings > Player > Android > Publishing Settings**.
2. Enable the following templates:
   - Custom Main Manifest
   - Custom Main Gradle Template
   - Custom Gradle Properties Template
   - Custom Gradle Settings Template
3. A folder named `Plugins/Android` should be generated with these files.

### Step 4: Import Plugins
#### Import External Dependency Manager
1. Import the `ExternalDependencyManager` package.
2. Ensure the `ExternalDependencyManager` and `PlayServicesResolver` folders are added to the Assets directory.
3. Restart Unity if the **Assets > External Dependency Manager** menu doesn’t appear.

#### Import AppsFlyer Plugin
1. Import the `AppsFlyer Unity Plugin v6.15.3`.
2. **Important**: Uncheck `ExternalDependencyManager` and `PlayServicesResolver` during import.
3. Enable auto-resolution when prompted to resolve dependencies and conflicts.

### Step 5: Add Your App to AppsFlyer
#### For Unpublished Apps:
1. Visit [AppsFlyer Dashboard](https://hq1.appsflyer.com/apps/myapps/new).
2. Provide your app’s package name.
3. Configure necessary details and copy the provided **Dev Key** for later use.

#### For Apps Published on Third-Party Stores:
1. Upload the app to a location like Dropbox or Google Drive.
2. Use the shared link as the app’s URL in AppsFlyer.
3. Follow the steps to configure and copy the **Dev Key**.

### Step 6: Create OneLink Template
1. Navigate to [OneLink Management](https://hq1.appsflyer.com/onelink).
2. Create a new OneLink template with a subdomain and desired redirection settings.
   - Example URI scheme for testing: `deeplinkwithoutgoogleplay://open`.
3. Save the template for future use.

### Step 7: Generate OneLink
1. In OneLink Management, click **New Link**.
2. Customize the experience under General Settings:
   - Add media source name and campaign details.
   - Provide branding for the short link if required.
3. Configure deep linking parameters and redirections.
4. Save the link for testing.

### Step 8: Implement DeepLinkManager in Unity
1. Create a `DeepLinkManager` script using the [provided code](https://gist.github.com/AryanDhrafani-Yudiz/3b9c9fe26989e79684a2ac5ef0f1008a).
2. Attach the script to an empty GameObject in the scene.
3. Configure the script:
   - Add the Dev Key and iOS App ID.
   - Enable debug mode during development.
4. Adjust the OneLink configuration for runtime deep link generation.

### Step 9: Update Android-Specific Files
#### Update `AndroidManifest.xml`
1. Use the [provided manifest file](https://gist.github.com/AryanDhrafani-Yudiz/55d6f93fb9aa26d76e9ba935104742d5).
2. Update the package name and deep linking intent for your subdomain or URI scheme.

#### Update Gradle Templates
1. Use the [provided gradleTemplate.properties](https://gist.github.com/AryanDhrafani-Yudiz/fc8a30a78c02b0dce0d604895d454878).
2. Use the [provided mainTemplate.gradle](https://gist.github.com/AryanDhrafani-Yudiz/259635bc955e6f8a317e298152397a99).
3. Update the [settingsTemplate.gradle](https://gist.github.com/AryanDhrafani-Yudiz/c28e118c114432312bc3edca3d43b5dc) with required dependencies.

### Step 10: Resolve Dependencies
1. Go to **Assets > External Dependency Manager > Android Resolver > Delete Resolved Libraries**.
2. Force resolve the dependencies to ensure the project is correctly configured.

### Step 11: Build and Test
1. Build the APK or iOS project.
2. Test the deep linking functionality by clicking generated links.

---

## Debugging Tips
1. Use the in-game debug console for real-time logs.
2. Validate deep link callbacks by parsing received arguments.
3. Ensure Gradle errors during the build are addressed by updating dependency versions.

---

## Notes
- HTTPS schemes work for apps published on official stores. Use URI schemes for unpublished apps.
- Modify parameters as needed for project-specific settings.

---

For additional support, refer to the official AppsFlyer documentation or contact their support team.
