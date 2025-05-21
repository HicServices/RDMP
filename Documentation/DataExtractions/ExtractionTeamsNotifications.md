## Extraction Teams Notification

RDMP has the ability to notify you of the status of an Extractions final result via Microsoft Teams. This may be useful for extractions that take a significant amount of time to complete, or are run from a secondary system.

### Prerequisites
* A Microsoft teams channel for notifications to be sent to
* Permission to create new workfows for said channel

### Setup
This integration uses the MS Teams Workflows App. It comes as standard with all Microsoft Teams instances.

### Step 1:
Within a Teams channel, select the three dots at the top right, there should be a "Workflows" menu item. Select this menu item.

### Step 2:
A dialog will appear, search for a workflow named "Post to a channel when a webhook request is received". Select this option.

### Step 3:
Give the workflow a sensible name i.e. "John Smith's  Extraction Notifications".
Follow through all the options presented over the next few pages and click "Add workflow" when it appears.
A URL will now be displayed. Copy this value. It can be retrieved again later via the "Manage Workflows" section within Microsoft teams.

### Step 4:
Within RDMP, open your user settings "View > User Settings" and populate the "Notification Webhook URL" Textbox with the URL from the previous step.
You will also need to provide the email address you use in Microsoft teams in the "Webhook Email Address" Textbox. This is to allow the notification to tag you when it posts.
Once these fields are populated, close the window and restart RDMP.
You should now recieve notifications about any Extractions you run within Microsoft Teams.