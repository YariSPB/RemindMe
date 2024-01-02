# RemindMe
MAUI .NET app to remind me things to do for Android phone

"Don't forget to buy milk on the way home", she asked. I replied "Sure", and I forgot. 

This article is about an experiment to make a location tracking app for my Android phone to notify me things to do on my way home. More importantly, to get first experience with MAUI .Net development, and share my thoughts. 

A user story is simple; record home GPS location at start, schedule recurring GPS location checks, and inform me via notification when I am within close proximity to home. As a result, I get notified with phone vibration sequence and instantly remember that I should do something before returning home. 

This works similarly to Neville Longbottom's "Rememberall", gifted by his grandma in the first book of Harry Potter.

I approached this task with MAUI .Net cross-platform development platform, and as a result, created and tested a prototype on my Android Phone. I was able to achieve desired functionality when the phone was in active state or just after being locked. However, it didn't work reliably in a locked (doze) state due to certain Android API and phone OS requirements.
