<h1>About</h1>
Esta es un kit de herramientas de analisis de volumen que comenze a programar en Febrero de 2020, esta enfocado a todos los mercados de Futuros y escrito en C#, Particularmente corre en la plataforma NinjaTrader8. Aunque creo que hay cosas que faltan agregar libero el codigo para ayudar a toda la comunidad de programadores interesados en los mercados financieros y el trading, saludos!
<br/>

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

If you see my examples all of it is "1 second" of time-frame, this is because with this setup we can able to see the market in a very precision way. Experts algorithms can use this for detect market manipulation and High frequency trading.

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

This tools offers a lot of styles and formulas to deal with it.
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



<h1>Volume Filter</h1>

<h1>Market Volume</h1>
