db.getCollection("Account").aggregate(
[
	{$unwind: "$InstagramAccs"},
	{$lookup: {from: "InstagramAccounts",localField: "InstagramAccs",foreignField: "_id",as: "InstagramAccount"}},
	{$unwind : "$InstagramAccount"},
	{$lookup: {from: "Profiles", localField: "InstagramAccount.Profile_using_id", foreignField: "_id", as: "InstagramAccount.Profile"}},
	{$lookup: {from: "Proxies", localField: "InstagramAccount.Proxy_using_id", foreignField: "_id", as: "Proxy"}},
	{$unwind: "$Proxy"},
	{$unwind: "$InstagramAccount.Profile"},
	{$lookup: {from: "Theme", localField: "InstagramAccount.Profile.Theme_using_id", foreignField: "_id", as: "Theme"}},
	{$unwind: "$Theme"}
]
)

db.getCollection("Profiles").aggregate([
	{$lookup: {from:"Theme", localField: "Theme_using_id", foreignField: "_id", as: "themey"}}
])