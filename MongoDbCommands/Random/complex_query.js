db.getCollection("Account").aggregate([
{$unwind: "$InstagramAccs"},
{$lookup: {from: "InstagramAccounts",localField: "InstagramAccs",foreignField: "_id",as: "InstaAccounts"}},
{$unwind : "$InstaAccounts"},
{$lookup: {from: "Profiles", localField: "InstaAccounts._Profile_using_id", foreignField: "_id", as: "profiles"}},
//{$unwind: "$profiles"},
{$lookup: {from:"Theme", localField:"profiles_.id", foreignField: "Theme_Using_Id", as: "theme"}},
{$lookup: {from:"Proxies", localField: "InstaAccounts._id", foreignField:"Proxy_using_id", as:"proxies"}}
])

{$group : { _id : "$_id", InstagramAccount : {$push : "$InstaAccounts"}, InstaIds: {$push : "$InstaAccounts._id"}}},
{$unwind : "$InstaIds"},