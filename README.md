# Timer

Easy helper app to start and stop timesheet entries.

## Installation

```bash
git clone https://github.com/leonbohmann/timer
```

```bash
cd timer
dotnet build
```

You can also publish the app to create a single executable file:

```bash
dotnet publish -c Release
```


## How it works

On your first start, you have to enter your credentials. For the server, enter the base adress (without **/api**):

```
https://kimai.your-domain.com
```

Also, insert your username and an API Token. For now, the API Token is stored in plaintext next to the executable.


You start by choosing a project and an activity. Then, there is a description box. As soon as you hit start,
a new entry is created and the entry-id is saved. Even if you restart the Timer-App, the most recent, not stopped 
entry is still *active*. You can stop the entry by hitting the stop button. The entered description is synced to
the entry on your server.