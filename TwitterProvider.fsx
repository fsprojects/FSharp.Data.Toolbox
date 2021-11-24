(**
[![Binder](img/badge-binder.svg)](https://mybinder.org/v2/gh/fsprojects/FSharp.Data.Toolbox/gh-pages?filepath=TwitterProvider.ipynb)&emsp;
[![Script](img/badge-script.svg)](/FSharp.Data.Toolbox//TwitterProvider.fsx)&emsp;
[![Notebook](img/badge-notebook.svg)](/FSharp.Data.Toolbox//TwitterProvider.ipynb)

F# Data Toolbox: Twitter type provider
========================

The Twitter type provider makes Twitter data easily accessible 
by providing a light wrapper around the Twitter API. 

Connecting to Twitter
-----------------------------------------
Twitter requires developers to register their applications to gain access
to its API. You have to register your application at
[Twitter Apps](https://apps.twitter.com/). 
After registration, Twitter provides API key and API secret to authenticate the application.

*)
#r "nuget: FSharp.Data.Toolbox.Twitter,0.20.2"
(**
You can use FSharp.Data.Toolbox in [dotnet interactive](https://github.com/dotnet/interactive) notebooks in [Visual Studio Code](https://code.visualstudio.com/) or [Jupyter](https://jupyter.org/), or in F# scripts (`.fsx` files), by referencing the package as follows:

    // Use one of the following two lines
    #r "nuget: FSharp.Data.Toolbox.Twitter" // Use the latest version
    #r "nuget: FSharp.Data.Toolbox.Twitter,0.20.2" // Use a specific version

*)
#r "nuget: FSharp.Data"

open FSharp.Data.Toolbox.Twitter
open FSharp.Data


let key = "mKQL29XNemjQbLlQ8t0pBg"
let secret = "T27HLDve1lumQykBUgYAbcEkbDrjBe6gwbu0gqi4saM"
(**
There are two types of possible connections to Twitter,
application-only and full OAuth authentication. They provide 
different access rights and different number of [allowed requests
per time window](https://dev.twitter.com/docs/rate-limiting/1.1/limits).

### Connecting with application-only authentication
The [application-only authentication](https://dev.twitter.com/docs/auth/application-only-auth)
 provides access that's 
limited to data reachable without the full user context.
For example, it allows accessing friends and followers and searching
in tweets.
This is how the application obtains acess credentials:  
*)
let twitter = Twitter.AuthenticateAppOnly(key, secret)
(**
### Connecting with OAuth
This method of access provides full user context for the application. 
Compared to application-only access, it can also access Streaming, 
search for users and post tweets on behalf of the user.

To connect with OAuth, we first create a Twitter connector.
with your user name and password. You'll get a PIN that you 
use as an argument for the Connect function. This user authentication
allows full access to Twitter APIs.

*)
let connector = Twitter.Authenticate(key, secret) 

// Run this part after you obtain PIN
let twitter = connector.Connect("8779691")
(**
![Twitter login window](img/OAuthWindow.png)

After connecting to Twitter, you can call methods to access 
Twitter users, list followers and friends, search for tweets and
access the global Twitter stream. All the methods send http requests
to Twitter and return JSON documents. They are parsed using JSON
type provider, which allows to access individual properties.

Accessing friends and followers
---------------------------------------
The following examples show how to access lists of followers
and friends (followed accounts). Users can be identified either
by their Twitter name, or by their user ID number.

*)
// Get a list of ID numbers of friends and followers 
// for the current signed-in user
// (requires full authentication)
let friends = twitter.Connections.FriendsIds()
let followers = twitter.Connections.FollowerIds()
printfn "Number of friends: %d" (friends.Ids |> Seq.length)
printfn "Number of followers: %d" (followers.Ids |> Seq.length)

// Get a list IDs of friends and followers for a specific user 
let followersFSorg = twitter.Connections.FriendsIds(userId=880772426L)
let friendsFSorg = twitter.Connections.FollowerIds(screenName="fsharporg")

// Get information about connection between specific users
let fs = twitter.Connections.Friendship(880772426L, 94144339L)
fs.Relationship.Source.ScreenName
fs.Relationship.Target.ScreenName
fs.Relationship.Source.Following
fs.Relationship.Source.FollowedBy
(**
We can also search for information about a list of users, specified 
either by IDs or by their screen names. It's possible to search for
up to 100 users at a time. 
*)
let friendInfos = twitter.Users.Lookup(friends.Ids |> Seq.truncate 100)
for friend in friendInfos do
  printfn "%s (@%s)\t\t%d" friend.Name friend.ScreenName friend.Id
(**
Searching for tweets
-----------------------------------
We can search Twitter for tweets using keywords. The following snippet shows
how to search for tweets containing the #fsharp tag. 

*)
let fsharpTweets = twitter.Search.Tweets("#fsharp", count=100)

for status in fsharpTweets.Statuses do
  printfn "@%s: %s" status.User.ScreenName status.Text
  
(**
Accessing timelines
--------------------------------------
Let's look at how to access Twitter timelines. Timelines show a stream
of tweets from followed accounts. 

We can access timelines for specific users, or the home timeline
of the current signed-in user. 
*)
// Access home timeline
// (requires full user authentication)
let home = twitter.Timelines.HomeTimeline()

// Timeline of a specific user, up to a specified number of tweets
let timeline = twitter.Timelines.Timeline("fsharporg", 10)
(**
We can display the Timeline in a web browser. We first create a
web browser window. Then we download timeline for a specific user,
in this case it's [@fsharporg](https://twitter.com/fsharporg). 
Finally, we display individual tweets in the web browser.
*)
// Display timeline
let timeline = twitter.Timelines.Timeline("fsharporg")
for tweet in timeline do
    printfn $"{tweet.User.Name}: {tweet.Text}" 
(**
Output:
```text
Hear from F# creator Don S… https://t.co/N8mYEXVPrv
fsharp.org: Announcing the Winter 2021 round of the #fsharp Mentorship program https://t.co/88zkM9YEgZ Applications being accepted now!
fsharp.org: Welcome to 2021! #fsharp https://t.co/wYu0MHw8hp
fsharp.org: @voh_ing We will figure it out for you - sorry for the confusion.
fsharp.org: Three videos on understanding the F# Compiler are now available on our YouTube channel https://t.co/gK2BTvyL8E #fsharp
fsharp.org: First free tickets to F# Exchange as part of the Diversity Program have already been allocated. If you are a member… https://t.co/cB4aDi6c4u
fsharp.org: We have 10 free tickets to F# Exchange available for members of any minority underrepresented in technology! Reach… https://t.co/Jy5qOHe5A0
fsharp.org: We are excited to see the return of the Applied F# Challenge for 2021! https://t.co/uNaEVZVkYt Call for judges is now open. #fsharp
fsharp.org: The F# Software Foundation is now accepting applications for the 9th round of the #fsharp mentorship program! https://t.co/ZieMmsL8pl
fsharp.org: Our Annual Members Meeting is starting now on Slack! All members are welcome to attend, we hope to see you there :) #fsharp
fsharp.org: With a slight delay, our newly elected Board will be revealed at the Annual Members Meeting, Aug 5th, 
hope to see y… https://t.co/rUeK7KJRMO
fsharp.org: Reminder: the annual FSSF Board election is underway!
If you have someone in mind to represent you and the F# Commu… https://t.co/in6FlHqNxX
fsharp.org: Our annual Board election is underway!
2020 being what it is, there are probably bigger things on your mind that th… https://t.co/g1FmcTQjCi
fsharp.org: Please read our full statement: https://t.co/eSOf6y5air  #fsharp #BlackLivesMatter
fsharp.org: Again, Black Lives Matter.
...
```
*)
// Access mentions timeline
// (requires full user authentication)
let mention = twitter.Timelines.MentionTimeline()
for tweet in mention do
    printfn $"{tweet.User.Name}: {tweet.Text}" 
(**
Streaming data 
--------------------------------------
Streaming allows access to live Twitter data, as they're posted. 
To access Streaming API, the application must have full user
authentication. 

If we reuse the web browser window created in the previous 
code sample, we can display a random sample of tweets in the following
way.
*)
// Display stream with live data
let subscribe f obs = 
  let ctx = System.Threading.SynchronizationContext.Current
  if ctx = null then obs |> Observable.subscribe f
  else obs |> Observable.subscribe (fun v -> ctx.Post((fun _ -> f v), null))

let sample = twitter.Streaming.SampleTweets()
sample.TweetReceived |> subscribe (fun status ->
    match status.Text, status.User with
    | text, user ->
        printfn $"{user.Name}: {text}"
    | _ -> ()  )

sample.Start()

sample.Stop()
(**
We can also search the Twitter stream for a specific hashtag or phrase. 
The following code will filter all tweets that contain the word "fsharp" 
from the global stream of tweets.
*)
// Display live search data 

let search = twitter.Streaming.FilterTweets ["fsharp"]
sample.TweetReceived |> subscribe (fun status ->
    match status.Text, status.User with
    | text, user ->
        printfn $"{user.Name}: {text}"
    | _ -> ()  )

search.Start()

search.Stop()

