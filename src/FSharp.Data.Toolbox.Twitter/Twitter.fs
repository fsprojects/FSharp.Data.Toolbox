#if INTERACTIVE
#I "../../packages/FSharp.Data/lib/netstandard2.0"
#r "FSharp.Data.dll"
#else
namespace FSharp.Data.Toolbox.Twitter
#endif

open System
open System.Globalization
open System.Threading
open System.Web
open System.IO
open System.Runtime.InteropServices
open System.Net
open System.Security.Cryptography
open System.Text
open FSharp.Data
open FSharp.Control



// ----------------------------------------------------------------------------------------------

module internal Utils =
  let requestTokenURI = "https://api.twitter.com/oauth/request_token"
  let accessTokenURI = "https://api.twitter.com/oauth/access_token"
  let authorizeURI = "https://api.twitter.com/oauth/authorize"
  let appOnlyTokenURI = "https://api.twitter.com/oauth2/token"

  type Response = JsonProvider<"json/bearer_token.json", EmbeddedResource="FSharp.Data.Toolbox.Twitter,bearer_token.json">
  
  // Utilities
  let unreservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";
  let urlEncode str =
      String.init (String.length str) (fun i ->
          let symbol = str.[i]
          if unreservedChars.IndexOf(symbol) = -1 then
              Encoding.UTF8.GetBytes [| symbol |]
              |> Array.map (fun n -> String.Format("%{0:X2}", n))
              |> String.concat ""
          else
              string symbol)

  // Core Algorithms
  let hmacsha1 signingKey str =
      let converter = new HMACSHA1(Encoding.ASCII.GetBytes(signingKey : string))
      let inBytes = Encoding.ASCII.GetBytes(str : string)
      let outBytes = converter.ComputeHash(inBytes)
      Convert.ToBase64String(outBytes)

  let encodeBearerToken tokenStr =
    let inBytes = Encoding.ASCII.GetBytes(tokenStr : string)
    Convert.ToBase64String(inBytes)

  let compositeSigningKey consumerSecret tokenSecret =
      urlEncode(consumerSecret) + "&" + urlEncode(tokenSecret)

  let baseString httpMethod baseUri queryParameters =
      httpMethod + "&" +
      urlEncode(baseUri) + "&" +
      (queryParameters
       |> Seq.sortBy (fun (k,v) -> k)
       |> Seq.map (fun (k,v) -> urlEncode(k)+"%3D"+urlEncode(v))
       |> String.concat "%26")

  let createAuthorizeHeader queryParameters =
      let headerValue =
          "OAuth " +
          (queryParameters
           |> Seq.map (fun (k,v) -> urlEncode(k)+"\x3D\""+urlEncode(v)+"\"")
           |> String.concat ",")
      headerValue

  let createAppOnlyAuthorizeHeader credentials =
      "Basic " + credentials

  let createAppOnlyRequestHeader accessToken =
      "Bearer " + accessToken

  let currentUnixTime() = floor (DateTime.UtcNow - DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds

  /// Request a token from Twitter and return:
  ///  oauth_token, oauth_token_secret, oauth_callback_confirmed
  let public requestToken consumerKey consumerSecret =
      let signingKey = compositeSigningKey consumerSecret ""

      let queryParameters =
          ["oauth_callback", "oob";
           "oauth_consumer_key", consumerKey;
           "oauth_nonce", System.Guid.NewGuid().ToString().Substring(24);
           "oauth_signature_method", "HMAC-SHA1";
           "oauth_timestamp", currentUnixTime().ToString();
           "oauth_version", "1.0"]

      let signingString = baseString "POST" requestTokenURI queryParameters
      let oauth_signature = hmacsha1 signingKey signingString

      let realQueryParameters = ("oauth_signature", oauth_signature)::queryParameters

      let req = WebRequest.Create(requestTokenURI, Method="POST")
      let headerValue = createAuthorizeHeader realQueryParameters
      req.Headers.Add(HttpRequestHeader.Authorization, headerValue)

      let resp = req.GetResponse()
      let stream = resp.GetResponseStream()
      let txt = (new StreamReader(stream)).ReadToEnd()

      let parts = txt.Split('&')
      (parts.[0].Split('=').[1],
       parts.[1].Split('=').[1],
       parts.[2].Split('=').[1] = "true")

  // Request an application-only Twitter bearer token
  // and returns the access token for the bearer
  // which is used to authenticate API requests
  let public requestAppOnlyToken consumerKey consumerSecret =
      let bearerToken = consumerKey + ":" + consumerSecret
      let encBearerToken = encodeBearerToken bearerToken

      let req = WebRequest.Create(appOnlyTokenURI, Method="POST")
      req.ContentType <- @"application/x-www-form-urlencoded;charset=UTF-8"
      let headerValue = createAppOnlyAuthorizeHeader encBearerToken
      req.Headers.Add(HttpRequestHeader.Authorization, headerValue)
      let bodyValue = "grant_type=client_credentials"
      req.GetRequestStream().Write(Encoding.ASCII.GetBytes(bodyValue), 0, bodyValue.Length)

      let resp = req.GetResponse()
      let stream = resp.GetResponseStream()
      let txt = (new StreamReader(stream)).ReadToEnd()
      let respContents = Response.Parse(txt)

      // Check if the token type is bearer
      if not (respContents.TokenType = "bearer") then
          printfn "Incorrect response token type, expecting \"bearer\""
      respContents.AccessToken

  /// Get an access token from Twitter and returns: oauth_token, oauth_token_secret
  let public accessToken consumerKey consumerSecret token tokenSecret verifier =
      let signingKey = compositeSigningKey consumerSecret tokenSecret

      let queryParameters =
          ["oauth_consumer_key", consumerKey;
           "oauth_nonce", System.Guid.NewGuid().ToString().Substring(24);
           "oauth_signature_method", "HMAC-SHA1";
           "oauth_token", token;
           "oauth_timestamp", currentUnixTime().ToString();
           "oauth_verifier", verifier;
           "oauth_version", "1.0"]

      let signingString = baseString "POST" accessTokenURI queryParameters
      let oauth_signature = hmacsha1 signingKey signingString

      let realQueryParameters = ("oauth_signature", oauth_signature)::queryParameters

      let req = WebRequest.Create(accessTokenURI, Method="POST")
      let headerValue = createAuthorizeHeader realQueryParameters
      req.Headers.Add(HttpRequestHeader.Authorization, headerValue)

      let resp = req.GetResponse()
      let stream = resp.GetResponseStream()
      let txt = (new StreamReader(stream)).ReadToEnd()

      let parts = txt.Split('&')
      (parts.[0].Split('=').[1],
       parts.[1].Split('=').[1])

  /// Compute the 'Authorization' header for the given request data
  let authHeaderAfterAuthenticated consumerKey consumerSecret originalUrl httpMethod token tokenSecret queryParams =
      let signingKey = compositeSigningKey consumerSecret tokenSecret

      let queryParameters =
              ["oauth_consumer_key", consumerKey;
               "oauth_nonce", System.Guid.NewGuid().ToString().Substring(24);
               "oauth_signature_method", "HMAC-SHA1";
               "oauth_token", token;
               "oauth_timestamp", currentUnixTime().ToString();
               "oauth_version", "1.0"]

      let signingQueryParameters =
          List.append queryParameters queryParams

      let signingString = baseString httpMethod originalUrl signingQueryParameters
      let oauth_signature = hmacsha1 signingKey signingString
      let realQueryParameters = ("oauth_signature", oauth_signature)::queryParameters
      let headerValue = createAuthorizeHeader realQueryParameters
      headerValue

  let addAuthHeaderForApp (webRequest : WebRequest) originalUrl appOnlyAccessToken queryParams =
      let httpMethod = webRequest.Method
      let headerValue = createAppOnlyRequestHeader appOnlyAccessToken
      webRequest.Headers.Add(HttpRequestHeader.Authorization, headerValue)

  /// Add an Authorization header to an existing WebRequest
  let addAuthHeaderForUser (webRequest : WebRequest) originalUrl consumerKey consumerSecret token tokenSecret queryParams =
      let httpMethod = webRequest.Method
      let header = authHeaderAfterAuthenticated consumerKey consumerSecret originalUrl httpMethod token tokenSecret queryParams
      webRequest.Headers.Add(HttpRequestHeader.Authorization, header)

  let makeParams list = List.concat list
  let inline optional key = function Some value -> [key, string value] | _ -> []
  let inline required key value = [key, string value]

  let openWebpage url =
      if RuntimeInformation.IsOSPlatform(OSPlatform.Windows) then
          let psi = System.Diagnostics.ProcessStartInfo(FileName = url, UseShellExecute = true)
          System.Diagnostics.Process.Start(psi) |> ignore
      elif RuntimeInformation.IsOSPlatform(OSPlatform.Linux) then
          System.Diagnostics.Process.Start("xdg-open", url) |> ignore
      elif RuntimeInformation.IsOSPlatform(OSPlatform.OSX) then
          System.Diagnostics.Process.Start("open", url) |> ignore
      else
          invalidOp "Not supported OS platform"
// ----------------------------------------------------------------------------------------------
type TwitterStream<'T> =
  abstract TweetReceived : IEvent<'T>
  abstract Stop : unit -> unit
  abstract Start : unit -> unit

type TwitterUserContext = {
    ConsumerKey : string;
    ConsumerSecret : string;
    AccessToken : string;
    AccessSecret : string }

type TwitterAppContext = {
    AppOnlyToken : string }

type TwitterContext =
  | UserContext of TwitterUserContext
  | AppContext of TwitterAppContext

[<AutoOpen>]
module WebRequestExtensions =
  type System.Net.WebRequest with
    /// Add an Authorization header to the WebRequest for the provided user authorization tokens and query parameters
    member this.AddOAuthHeader(consumerKey, consumerSecret, userToken, userTokenSecret, queryParams, ?originalUrl) =
      let originalUrl = defaultArg originalUrl (this.RequestUri.ToString())
      Utils.addAuthHeaderForUser this originalUrl consumerKey consumerSecret userToken userTokenSecret queryParams

    /// Add an Authorization header to the WebRequest for the provided user authorization tokens and query parameters
    member this.AddOAuthHeader(ctx:TwitterContext, queryParams, ?originalUrl) =
      let originalUrl = defaultArg originalUrl (this.RequestUri.ToString())
      match ctx with
      | AppContext {AppOnlyToken = appToken} ->
          Utils.addAuthHeaderForApp this originalUrl appToken queryParams
      | UserContext {ConsumerKey = ck; ConsumerSecret = cs; AccessToken = ack; AccessSecret = acs} ->
          Utils.addAuthHeaderForUser this originalUrl ck cs ack acs queryParams

// ----------------------------------------------------------------------------------------------

module TwitterTypes =
  type Tweet = JsonProvider<"json/stream.json", SampleIsList=true, EmbeddedResource="FSharp.Data.Toolbox.Twitter,FSharp.Data.Toolbox.Twitter.json.stream.json">
  type TimeLine = JsonProvider<"json/timeline.json", EmbeddedResource="FSharp.Data.Toolbox.Twitter,FSharp.Data.Toolbox.Twitter.json.timeline.json">
  type SearchTweets = JsonProvider<"json/search_tweets.json", EmbeddedResource="FSharp.Data.Toolbox.Twitter,FSharp.Data.Toolbox.Twitter.json.search_tweets.json">
  type IdsList = JsonProvider<"json/idslist.json", EmbeddedResource="FSharp.Data.Toolbox.Twitter,FSharp.Data.Toolbox.Twitter.json.idslist.json">
  type UsersLookup = JsonProvider<"json/users_lookup.json", EmbeddedResource="FSharp.Data.Toolbox.Twitter,FSharp.Data.Toolbox.Twitter.json.users_lookup.json">
  type FriendshipShow = JsonProvider<"json/friendship_show.json", EmbeddedResource="FSharp.Data.Toolbox.Twitter,FSharp.Data.Toolbox.Twitter.json.friendship_show.json">
  type MentionsTimeLine = JsonProvider<"json/mentions_timeline.json", EmbeddedResource="FSharp.Data.Toolbox.Twitter,FSharp.Data.Toolbox.Twitter.json.mentions_timeline.json">
  type TrendsAvailable = JsonProvider<"json/trends_available.json", EmbeddedResource="FSharp.Data.Toolbox.Twitter,FSharp.Data.Toolbox.Twitter.json.trends_available.json">
  type TrendsPlace = JsonProvider<"json/trends_place.json", EmbeddedResource="FSharp.Data.Toolbox.Twitter,FSharp.Data.Toolbox.Twitter.json.trends_place.json">
  type TrendsClosest = JsonProvider<"json/trends_closest.json", EmbeddedResource="FSharp.Data.Toolbox.Twitter,FSharp.Data.Toolbox.Twitter.json.trends_closest.json">
  type PostUpdate = JsonProvider<"json/post_update.json", EmbeddedResource="FSharp.Data.Toolbox.Twitter,FSharp.Data.Toolbox.Twitter.json.post_update.json">

type TwitterConnector =
  abstract Connect : string -> Twitter

and Streaming(context:TwitterContext) =
    member private tweets.downloadTweets (req:WebRequest) =
        let cts = new CancellationTokenSource()
        let event = new Event<_>()
        let downloadLoop = async {
          System.Net.ServicePointManager.Expect100Continue <- false
          let! resp = req.AsyncGetResponse()
          (* | :? WebException as ex ->
                let x = ex.Response :?> HttpWebResponse
                if x.StatusCode = HttpStatusCode.Unauthorized then
                    // TODO need inform user login has failed and they need to try again
                    printfn "Here?? %O" ex
                reraise() *)
          use stream = resp.GetResponseStream()
          use reader = new StreamReader(stream)

          while not reader.EndOfStream do
            let sizeLine = reader.ReadLine()
            if not (String.IsNullOrEmpty sizeLine) then
              let size = int sizeLine
              let buffer = Array.zeroCreate size
              let _numRead = reader.ReadBlock(buffer,0,size)
              let text = new System.String(buffer)
              event.Trigger(TwitterTypes.Tweet.Parse(text)) }

        { new TwitterStream<_> with
            member x.Start() = Async.Start(downloadLoop , cts.Token)
            member x.Stop() = cts.Cancel()
            member x.TweetReceived = event.Publish }

    member tweets.SampleTweets () =
      match context with
      | UserContext(c) ->
          let req = WebRequest.Create("https://stream.twitter.com/1.1/statuses/sample.json", Method="POST", ContentType = "application/x-www-form-urlencoded")
          req.AddOAuthHeader(context, ["delimited", "length"])
          req.Timeout <- 10000
          do use reqStream = req.GetRequestStream()
             use streamWriter = new StreamWriter(reqStream)
             streamWriter.Write(sprintf "delimited=length")
          tweets.downloadTweets req
      | _ -> failwith "Full user authentication is required to access Twitter Streaming."

    member tweets.FilterTweets keywords =
      match context with
      | UserContext(c) ->
          let query = String.concat "," keywords
          let req = WebRequest.Create("https://stream.twitter.com/1.1/statuses/filter.json", Method="POST", ContentType = "application/x-www-form-urlencoded")
          req.AddOAuthHeader(context, ["delimited", "length"; "track", Utils.urlEncode query])
          req.Timeout <- 10000
          do use reqStream = req.GetRequestStream()
             use streamWriter = new StreamWriter(reqStream)
             streamWriter.Write(sprintf "delimited=length&track=%s" (Utils.urlEncode query))
          tweets.downloadTweets req
      | _ -> failwith "Full user authentication is required to access Twitter Streaming."

and Timelines(context:TwitterContext) =
    member tl.HomeTimeline (?count:int, ?sinceId:int64, ?maxId:int64, ?trimUser:bool, ?contributorDetails:bool, ?includeEntities:bool) =
      let args =
        [ Utils.optional "count" count;
          Utils.optional "since_id" sinceId;
          Utils.optional "max_id" maxId;
          Utils.optional "trim_user" trimUser;
          Utils.optional "contributor_details" contributorDetails;
          Utils.optional "include_entities" includeEntities]
          |> Utils.makeParams
      match context with
      | UserContext(c) ->
          let res = TwitterRequest(context, "GET").RequestRawData("https://api.twitter.com/1.1/statuses/home_timeline.json", args)
          TwitterTypes.TimeLine.Parse(res)
      | _ -> failwith "Full user authentication is required to access Twitter Timelines."


    member tl.Timeline (userId: int64, ?count:int, ?maxId:int64) =
      let args = [ Utils.required "user_id" userId;
                   Utils.optional "count" count;
                   Utils.optional "max_id" maxId ]
                 |> Utils.makeParams
      let res = TwitterRequest(context, "GET").RequestRawData("https://api.twitter.com/1.1/statuses/user_timeline.json", args)
      TwitterTypes.TimeLine.Parse(res)

    member tl.Timeline (screenName: string, ?count:int, ?maxId:int64) =
      let args = [ Utils.required "screen_name" screenName;
                   Utils.optional "count" count;
                   Utils.optional "max_id" maxId ]
                 |> Utils.makeParams
      let res = TwitterRequest(context, "GET").RequestRawData("https://api.twitter.com/1.1/statuses/user_timeline.json", args)
      TwitterTypes.TimeLine.Parse(res)

    member tl.MentionTimeline (?count:int, ?sinceId:int64, ?maxId:int64, ?trimUser:bool, ?contributorDetails:bool, ?includeEntities:bool) =
      let args =
        [ Utils.optional "count" count;
          Utils.optional "since_id" sinceId;
          Utils.optional "max_id" maxId;
          Utils.optional "trim_user" trimUser;
          Utils.optional "contributor_details" contributorDetails;
          Utils.optional "include_entities" includeEntities]
          |> Utils.makeParams
      let res = TwitterRequest(context, "GET").RequestRawData("https://api.twitter.com/1.1/statuses/mentions_timeline.json", args)
      TwitterTypes.MentionsTimeLine.Parse(res)

and Search (context:TwitterContext) =
    member s.Tweets (query:string, ?lang:string, ?geocode:string, ?locale:string,
                          ?count:int, ?sinceId:int64, ?maxId:int64, ?until:string) =
      let args =
        [ Utils.required "q" query;
          Utils.optional "lang" lang;
          Utils.optional "geocode" geocode;
          Utils.optional "locale" locale;
          Utils.optional "count" count;
          Utils.optional "since_id" sinceId;
          Utils.optional "max_id" maxId;
          Utils.optional "until" until ]
          |> Utils.makeParams
      let res = TwitterRequest(context, "GET").RequestRawData("https://api.twitter.com/1.1/search/tweets.json", args)
      TwitterTypes.SearchTweets.Parse(res)

and Connections (context:TwitterContext) =
    member f.FriendsIds (?userId:int64, ?screenName:string, ?cursor:int64, ?count:int) =
      let args =
        [ Utils.optional "user_id" userId;
          Utils.optional "screen_name" screenName
          Utils.optional "cursor" cursor;
          Utils.optional "count" count ]
          |> Utils.makeParams
      let res = TwitterRequest(context, "GET").RequestRawData("https://api.twitter.com/1.1/friends/ids.json", args)
      TwitterTypes.IdsList.Parse(res)

    member f.FollowerIds (?userId:int64, ?screenName:string, ?cursor:int64, ?count:int) =
      let args =
        [ Utils.optional "user_id" userId;
          Utils.optional "screen_name" screenName
          Utils.optional "cursor" cursor;
          Utils.optional "count" count ]
          |> Utils.makeParams
      let res = TwitterRequest(context, "GET").RequestRawData("https://api.twitter.com/1.1/followers/ids.json", args)
      TwitterTypes.IdsList.Parse(res)

    member f.Friendship(?sourceId:int64, ?targetId:int64) =
      let args =
        [ Utils.optional "source_id" sourceId;
          Utils.optional "target_id" targetId ]
        |> Utils.makeParams
      let res = TwitterRequest(context, "GET").RequestRawData("https://api.twitter.com/1.1/friendships/show.json", args)
      TwitterTypes.FriendshipShow.Parse(res)

and Trends (context:TwitterContext) =
    member t.Available () =
      let res = TwitterRequest(context, "GET").RequestRawData("https://api.twitter.com/1.1/trends/available.json", [])
      TwitterTypes.TrendsAvailable.Parse(res)

    member t.Place (woeid:int, ?exclude:string) =
      let args =
        [ Utils.required "id" woeid
          Utils.optional "exclude" exclude  ]
        |> Utils.makeParams
      let res = TwitterRequest(context, "GET").RequestRawData("https://api.twitter.com/1.1/trends/place.json", args)
      TwitterTypes.TrendsPlace.Parse(res)

    member t.Closest (lat:float, long:float) =
      let args =
        [ Utils.required "lat" lat
          Utils.required "long" long ]
        |> Utils.makeParams
      let res = TwitterRequest(context, "GET").RequestRawData("https://api.twitter.com/1.1/trends/closest.json", args)
      TwitterTypes.TrendsClosest.Parse(res)

and Users (context:TwitterContext) =
    member t.Lookup userIds =
      if (Seq.length userIds) > 100 then
        failwith "Lookup possible only for up to 100 users in one request"
      let args = [ "user_id", [ for (i:int64) in userIds -> string i ] |> String.concat "," ]
      let res = TwitterRequest(context, "GET").RequestRawData("https://api.twitter.com/1.1/users/lookup.json", args)
      TwitterTypes.UsersLookup.Parse(res)

    member t.Lookup screenNames =
      if (Seq.length screenNames) > 100 then
        failwith "Lookup possible only for up to 100 users in one request"
      let args = [ "screen_name", screenNames |> String.concat "," ]
      let res = TwitterRequest(context, "GET").RequestRawData("https://api.twitter.com/1.1/users/lookup.json", args)
      TwitterTypes.UsersLookup.Parse(res)

and Tweet (context:TwitterContext) =
    member t.Post (status:string, ?in_reply_to_status_id:int64, ?possibly_sensitive:bool, ?lat:float,
                          ?long:float, ?place_id:string, ?display_coordinates:bool, ?trim_user:bool, ?media_ids:int64[]) =
      match context with
      | UserContext _ ->
          let args =
            [ Utils.required "status" status;
              Utils.optional "in_reply_to_status_id" in_reply_to_status_id;
              Utils.optional "possibly_sensitive" possibly_sensitive;
              Utils.optional "lat" lat;
              Utils.optional "long" long;
              Utils.optional "place_id" place_id;
              Utils.optional "display_coordinates" display_coordinates;
              Utils.optional "trim_user" trim_user;
              Utils.optional "media_ids" media_ids
              ]
              |> Utils.makeParams
          let res = TwitterRequest(context, "POST").RequestRawData("https://api.twitter.com/1.1/statuses/update.json", args)
          TwitterTypes.PostUpdate.Parse(res)
      | _ -> failwith "Full user authentication is required to access Twitter Timelines."

and TwitterRequest(context:TwitterContext, httpmethod) =
  member twitter.RequestRawData (url:string, ?query) =
    let query = defaultArg query []
    let query = [ for k, v in query -> k, Utils.urlEncode v ]
    let queryString = [for k, v in query -> k + "=" + v] |> String.concat "&"
    let req = WebRequest.Create(url + (if query <> [] then "?" + queryString else ""), Method=httpmethod)
    req.AddOAuthHeader(context, query, url)

    use resp = req.GetResponse()
    use strm = resp.GetResponseStream()
    use sr = new StreamReader(strm)
    sr.ReadToEnd()

and Twitter(context:TwitterContext) =

  static member Authenticate(consumer_key, consumer_secret) =
    let request_token, request_secret, _ = Utils.requestToken consumer_key consumer_secret
    let url = Utils.authorizeURI + "?oauth_token=" + request_token
    Utils.openWebpage url
    { new TwitterConnector with
        member x.Connect(number) =
          let access_token, access_secret =
            Utils.accessToken consumer_key consumer_secret request_token request_secret number
          Twitter (
            UserContext {
              ConsumerKey = consumer_key;
              ConsumerSecret = consumer_secret;
              AccessToken = access_token;
              AccessSecret = access_secret; })
    }

  static member AuthenticateAppOnly (consumer_key, consumer_secret) =
    let token = Utils.requestAppOnlyToken consumer_key consumer_secret
    Twitter(AppContext { AppOnlyToken = token })

  static member AuthenticateAppSingleUser (consumer_key, consumer_secret, access_token, access_secret) =
    Twitter(
      UserContext {
              ConsumerKey = consumer_key;
              ConsumerSecret = consumer_secret;
              AccessToken = access_token;
              AccessSecret = access_secret; })

  member twitter.Timelines = Timelines(context)
  member twitter.Streaming = Streaming(context)
  member twitter.Search = Search(context)
  member twitter.Connections = Connections(context)
  member twitter.Users = Users(context)
  member twitter.Trends = Trends(context)
  member twitter.Tweets = Tweet(context)
  member twitter.RequestRawData(url:string, query) = TwitterRequest(context, "GET").RequestRawData(url, query)
