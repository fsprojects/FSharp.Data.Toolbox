#I "../../bin/"
#I "../../packages/"

#r @"FSharp.Data.2.0.7\lib\net40\FSharp.Data.dll"
#r "FSharp.Data.Toolbox.Twitter.dLl"

open FSharp.Data.Toolbox.Twitter

open System
open System.Threading
open System.Windows.Forms
open System.Collections.Generic

open FSharp.Control
open FSharp.WebBrowser
open FSharp.Data.Toolbox.Twitter

// ----------------------------------------------------------------------------
// Connecting to Twitter
// ----------------------------------------------------------------------------

// Access credentials
let key = "CoqmPIJ553Tuwe2eQgfKA"
let secret = "dhaad3d7DreAFBPawEIbzesS1F232FnDsuWWwRTUg"

// A) Full authentication
let connector = Twitter.Authenticate(key, secret) 

// NOTE: Run all code up to this point. A window should appear. You can then
// login to twitter and you'll get a pin code that you need to copy and
// paste as an argument to the 'Connect' method below:

let twitter = connector.Connect("6178040")

// B) Application-only authentication
let twitterApp = Twitter.AuthenticateAppOnly(key, secret)

// ----------------------------------------------------------------------------
// Using Twitter APIs
// ----------------------------------------------------------------------------

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

// Search for information on users
// (up to 100 users at a time)
let friendInfos = twitter.Users.Lookup(friends.Ids |> Seq.truncate 100)
for friend in friendInfos do
  printfn "%s (@%s)\t\t%d" friend.Name friend.ScreenName friend.Id

// Search for tweets
let fsharpTweets = twitter.Search.Tweets("#fsharp", count=100)
for status in fsharpTweets.Statuses do
  printfn "@%s: %s" status.User.ScreenName status.Text

// Access home timeline
// (requires full user authentication)
let home = twitter.Timelines.HomeTimeline()

// Timeline of a specific user, up to a specified number of tweets
let timeline = twitter.Timelines.Timeline("fsharporg", 10)

// Display timeline in a web browser
// - create a web browser
let frm = new Form(TopMost = true, Visible = true, Width = 500, Height = 400)
let btn = new Button(Text = "Pause", Dock = DockStyle.Top)
let web = new WebBrowser(Dock = DockStyle.Fill)
frm.Controls.Add(web)
frm.Controls.Add(btn)
web.Output.StartList()

// - display timeline
let timelineFSorg = twitter.Timelines.Timeline("fsharporg")
for tweet in timelineFSorg do
    web.Output.AddItem "<strong>%s</strong>: %s" tweet.User.Name tweet.Text

// Display stream with live data
web.Output.StartList()

let sample = twitter.Streaming.SampleTweets()
sample.TweetReceived |> Observable.guiSubscribe (fun status ->
    match status.Text, status.User with
    | Some text, Some user ->
        web.Output.AddItem "<strong>%s</strong>: %s" user.Name text
    | _ -> ()  )
sample.Start()

sample.Stop()

// Display live search data 
web.Output.StartList()

let search = twitter.Streaming.FilterTweets ["fsharp"]
search.TweetReceived |> Observable.guiSubscribe (fun status ->
    match status.Text, status.User with
    | Some text, Some user ->
        web.Output.AddItem "<strong>%s</strong>: %s" user.Name text
    | _ -> ()  )
search.Start()

search.Stop()

(*
For testing purposes, call other things...

twitter.RequestRawData("https://api.twitter.com/1.1/search/tweets.json", ["q", "fsharp"])
twitter.RequestRawData("https://api.twitter.com/1.1/followers/ids.json")
twitter.RequestRawData("https://api.twitter.com/1.1/users/lookup.json", ["user_id", "880772426,464211199"])
|> (fun s -> System.IO.File.WriteAllText(System.IO.Path.Combine(__SOURCE_DIRECTORY__, "references/users_lookup.json"), s))
*)