<h1>About</h1>
This is a volume analysis toolkit that I started programming in February 2020, it is focused on all Futures markets and written in C#, Particularly runs on the NinjaTrader8 platform, and it is based on the Wyckoff method. Although I think there are more things that can be added I release the code to help the whole community of programmers interested in financial markets and trading, greetings!
<br/>

<h3>A little of history about the proyect...</h3>
In mid of 2019 I work in my first investment fund that just started, I learned everything related to financial markets and how a trader operates, then about Dark Pools, HFT(High Frequency trading), Futures(Ninjatrader8), Forex(Metatrader5) and how Market makers work.

During that time I started to develop the first stage of the Order Flow, which I did from scratch.By that time I already understood the contract and volume mechanisms offered by Futures and all these financial instruments, so after many tests and headaches I managed to capture this information to do Type-Reading, Volume Profile, Book Mapping, etc... understanding the Wyckoff method was useful in the beginning but I was falling short, due to the frenzy of modern markets (by the increase of the underlying volatility and amount of robots), more latter i start to research as a freelancer in this area using Machine Learning, Deep Learning and Data Science.

<h2>Social media</h2>

![ins](/crypto_imgs/Instagram.png) Instagram: www.instagram.com/gab.zenobi/
<br/>
![lin](/crypto_imgs/linkedin.png) LinkedIn: www.linkedin.com/in/gabriel-zenobi

<h2>Contribute to this project</h2>
if you want to contribute, you can donate to any of the following addresses(Binance).

![btc](/crypto_imgs/bitcoin.png) Coin: BTC(Bitcoin), Network: _Bitcoin_
<br/>
Address: **1GgmkVTy7EjnuawvC24aUCmpFz48MBJqQm**

![eth](/crypto_imgs/ethereum.png) Coin: ETH(Ethereum), Network: BEP20 / ERC20
<br/>
Address: **0x5fc50c6108102c7d754471ce0db35b4ca3c19b34**
<br/>
or Network: _BEP2_, with Address: **bnb136ns6lfw4zs5hg4n85vdthaad7hq5m4gtkgf23**, and MEMO: **461252966**

![usdt](/crypto_imgs/usdt_theter.png) Coin: USDT(TetherUS),  Network: BEP20 / ERC20
<br/>
Address: **0x5fc50c6108102c7d754471ce0db35b4ca3c19b34**
<br/>
or Network: _TRC20_, with Address: **TLu3KdjysbVi5uabawzerpqRSS3qZxNXQm**
<br/>
or Network: _BEP2_, with Adress: **bnb136ns6lfw4zs5hg4n85vdthaad7hq5m4gtkgf23**, and MEMO: **461252966**

![bnb](/crypto_imgs/bnb.png) Coin: BNB, Network: BEP20 / ERC20
<br/>
Address: **0x5fc50c6108102c7d754471ce0db35b4ca3c19b34**
<br/>
or Network: _BEP2_, with Adress: **bnb136ns6lfw4zs5hg4n85vdthaad7hq5m4gtkgf23**, and MEMO: **461252966**

![ada](/crypto_imgs/ada_cardano.png) Coin: ADA(Cardano), Network: ADA Cardano
<br/>
Address: **DdzFFzCqrhsqz8XRBQQ8MQBNBbRjKXykmtxsL86JHfGGWfeA4DFMK5Qp8Byzj4PcrxoVxUyad3T413Fktxg1YzTh6nEfR3HXMcueDFtk**
<br/>
or Network: _BEP20_, with Address: **0x5fc50c6108102c7d754471ce0db35b4ca3c19b34**
<br/>
or Network: _BEP2_, with Address: **bnb136ns6lfw4zs5hg4n85vdthaad7hq5m4gtkgf23**, and MEMO: **461252966**

![busd](/crypto_imgs/BUSD.png) Coin: BUSD, Network: BEP20, ERC20
<br/>
Address: **0x5fc50c6108102c7d754471ce0db35b4ca3c19b34**
<br/>
or Network: _BEP2_, with Address: **bnb136ns6lfw4zs5hg4n85vdthaad7hq5m4gtkgf23**, and MEMO: **461252966**

<h2>How install</h2>
The fist step is to copy the files on AddOns folder and then paste it in the path AddOns of Ninjatrader8.

![addons](/how_install/addons.png)

The we need to do the same for the indicators(Bookmap.cs, MarketVolume.cs, OrderFlow.cs, VolumeAnalysisProfile.cs, VolumeFilter.cs). We paste it in the Ninjatrader8 indicators folder

![addons](/how_install/indicators.png)

Finally we need to activate "Tick Replay": Tools->Options->Market Data and activate "Show Tick Replay".

![opts](/how_install/tick_replay_enabled.png)

The same for Data Series of each Chart we use: New->Chart->Instrument(for example ES) and activate "Tick Replay".

![ds](/how_install/tick_replay_data_series.png)

<h1>Bookmap + Order book</h1>
The bookmap serves to see the depth of market (DOM or Level 2) using a heat map, which is useful to detect capital injections, aggressive and passive orders, aggressive levels of volume and also comes with an Order Book that shows the distribution of the price along with the total amount of volume in the market (both Total and Bid/Ask).

It is a very complete tool because it allows us to apply filters of orders and data to numerical and percentage level, plus a wide range of styles that lets us vary from colors to size of the letters and limits, but that's not all ... There is an option called "Realtime save session" within "Book Map Database" with which we can record in real time all the data extracted by the Bookmap and then save it in a .db file, this is very useful because we can reload the session whenever we want and not lose any progress. Another great feature is to be able to use this file (if you know how to read it) for data analysis, applying Machine Learning or any statistical method to look for patterns, in fact this is the purpose of such functionality.

If you see my examples all of it is "1 second" of time-frame, this is because with this setup we can able to see the market in a very precision way. Experts algorithms can use this for detect market manipulation and High frequency trading(HFT).

Features:
* Ladder range: x(levels of the ladder of contracts)
* Market orders calculation: Delta, Total.
Book map database:
* Session save file path: filename.db(the ".db" is obligatory)
* Session load file path: filename.db(the name of the file saved previously)
* Realtime save session: True/Falta(do you want to save?, enable this option!)
Book map Filters:
* Agressive market orders: X(detect agressive volume from X, this is show in the price)
* Big pending orders: Y(detect orders from Y, it is useful for detect HFT)

There are many other features that have to do with the tool's style and colors

![bookmap1](/book_map_imgs/setup.png)

Demo of default config of BookMap.

![bookmap2](/book_map_imgs/zoom.png)

Tunning ladder for see Bid/Ask separatly.

![bookmap3](/book_map_imgs/3_delta_and_total.png)

Zoom for show number of volume.

![bookmap4](/book_map_imgs/4_delta_and_total.png)

Another zoom for see big volume of orders(setting a pre-defined filter)

![bookmap5](/book_map_imgs/2_bid_and_ask.png)

<h1>Order Flow</h1>
The order flow is a tool that allows us to see the flow of orders in the market, with this we can see the difference between
the level of supply and demand. A good trader(or an AI algorithm in my case) can provide us information about a possible future price imbalance, and using this knowledge exploit a resistance point at which to place an order.
In other words, the order flow shows us the number of contracts at each price level, which is why it is sometimes also called "Cluster Chart" or "Footprint".

This tools offers a lot of styles and formulas to deal with it:
* Formulas: Bid/Ask, Total Delta, Total, Delta
* Representation: Volume(raw numbers), Percent
* Position(Profile): Left, Right
* Style: Profile, Heatmap
* Cluster POC, POC Lines, Cluster POI, POI Lines
* Extra info: POC = Point of control(Maximum volume cluster), POI = Point of imbalance(Minimum volume cluster)
The POI is a concept that i create, and just is the oppose to the POC...

![orderflow1](/order_flow_imgs/setup.png)

Demo of default config of the Order Flow/Footprint/Cluster chart(using BidAsk formula).

![orderflow2](/order_flow_imgs/bidask_volume_poc_poi.png)

Cluster POC + Cluster POI + POC and POI Lines(using TotalDelta formula)

![orderflow3](/order_flow_imgs/total_delta_cluster_poc_poi_lines.png)

Zooming the cluster we can see the contracts(using TotalDelta formula)

![orderflow4](/order_flow_imgs/total_delta_volume.png)

![orderflow5](/order_flow_imgs/bidask_volume_poc_poi2.png)

Using Heatmap style(BidAsk formula)

![orderflow6](/order_flow_imgs/bidask_volume_poc_poi_heatmap2.png)

Using Heatmap style(Total formula)

![orderflow7](/order_flow_imgs/total_volume_poc_poi_heatmap.png)

![orderflow8](/order_flow_imgs/total_volume_poc_poi_heatmap2.png)

<h1>Volume Profile</h1>

The volume profile shows a horizontal distribution of the number of contracts traded at each price level over a period of time. It also provides additional information such as POC, POI, Delta volume, Total Volume over a certain period of time.
One of its features is that it gives us the possibility to drag and drop, so we can select price zones and make a volume profile (we can also delete them when we don't need them).

Features:
* Formulas: Total, Delta, BidAsk, TotalAndBidAsk, TotalAndDelta, TotalAndDeltaAndBidAskm
* Ladder information: BidAsk, TotalAndDelta, Total, Delta.
* Time: Days, Minutes, Hours
* POC and POI.
* Hold CTRL and select a zone in the chart(the selected area will be marked in blue), then left click to show a new Volume Profile.
* Hold SHIFT and select an added Volume Profile(if the selected area is correct will be marked in red), then left click to delete a Volume Profile.

![volumeprofile1](/volume_analysis_profile_imgs/setup.png)
![volumeprofile2](/volume_analysis_profile_imgs/props.png)

Default config(BidAsk formula).

![volumeprofile3](/volume_analysis_profile_imgs/bidask_poc_poi.png)

Total and BidAsk formula

![volumeprofile4](/volume_analysis_profile_imgs/totalbidask_poc_poi.png)

Zooming the ladder of prices to see the volume.

![volumeprofile5](/volume_analysis_profile_imgs/ladder_information.png)

Only Delta volume calculations

![volumeprofile6](/volume_analysis_profile_imgs/delta_poc_poi.png)

Select a zone to add a new volume profile.

![volumeprofile7](/volume_analysis_profile_imgs/press_ctrl_and_drag_before.png)

Select a volume profile to delete.

![volumeprofile8](/volume_analysis_profile_imgs/pos_and_shift.png)

Total and Delta and BidAsk volume

![volumeprofile9](/volume_analysis_profile_imgs/total_delta_bidask_poc_poi.png)

<h1>Volume Filter</h1>

The volume filter is an indispensable weapon when we need to detect areas of important volume, probably the market is in a battle between buyers and sellers. Sometimes we will only filter out a specific amount of volume while at other times we will need a "map" of all incoming volume (from the smallest to the most aggressive). This tool is very useful if is combined, for example with the Book Map.

Features:
* Formula: Delta, Total.
* Min volume filter: X(a volume less than this amount will be discarted)
Geometry:
* Drawn: Circle, Rectangle.
* Fill: True, False.
* Agressive level: X(this impact on the size of the volume zone).
* Among others...

Default setup
![volumefilter1](/volume_filter_imgs/setup.png)

Filtering volume using Delta formula.

![volumefilter2](/volume_filter_imgs/delta_circle_fill.png)

Zooming

![volumefilter3](/volume_filter_imgs/delta_circle_nofill.png)

Using rectangles geometry

![volumefilter4](/volume_filter_imgs/delta_rect_nofill2.png)

Using Circles geometry with Total formula

![volumefilter5](/volume_filter_imgs/total_circle_nofill.png)

<h1>Market Volume</h1>

This is the simplest volume analysis tool, and one of the most used... with it we can see the total amount of Delta volume, accumulated volume among other characteristics. One of the most innovative things is that we can choose cumulative time-frames (most tools of this type only allow a fixed time) as a default period. So we can set volume time-frames.

Features:
* Formula: BidAsk, Delta, Total, TotalBidAsk, TotalDelta.
* Period: 1(by default, if we increase this amount we can accumulate the volume period)
* Market calculation style: Zero Line Color

Default setup.

![marketvolume1](/market_volume_imgs/setup.png)

Using Delta formula, Periods: 5

![marketvolume2](/market_volume_imgs/delta.png)

Using Total formula, Periods: 5

![marketvolume3](/market_volume_imgs/total.png)

Using TotalDelta formula, Periods: 5

![marketvolume4](/market_volume_imgs/total_delta.png)

Using TotalBidAsk formula, Periods: 5

![marketvolume5](/market_volume_imgs/total_bid_ask.png)
