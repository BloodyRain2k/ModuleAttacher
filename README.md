ModuleAttacher
==============

The main purpose of this mod is that you don't have to add parts to your ship to get functions that should be built in.
And example are the TAC Fuel Balancer or the Crew Manifest. These would have to be added by adding a new part to your ship, but with this mod you can let it add these to any parts you like.

In it's configfile PluginData\moduleattacher\ModuleAttacher.cfg you can enter the rules for adding modules to other parts.
Currently it's rather limited but it's sufficent for my current needs so it's fine.

Example 1:
TacFuelBalancer:ModuleCommand
This line will add the TAC Fuel Balancer to all command modules, remember which one you used, they are independant of each other. Also for docked crafts are the own tanks first in the list it seems.

Example 2:
CrewManifestModule:ModuleCommand["CrewCapacity">"0"]
This line will add the CrewManifest to all command pods that have crew capacity, you can theoretically query any property the match module (ModuleCommand in this case) has but currently it's limited to the type System.Int32.

The only thing you need to install for this mod is it's dll which you can download under https://github.com/BloodyRain2k/ModuleAttacher/raw/master/bin/Debug/ModuleAttacher.dll
