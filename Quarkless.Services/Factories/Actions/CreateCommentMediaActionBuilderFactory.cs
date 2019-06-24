﻿using System;
using System.Collections.Generic;
using System.Text;
using Quarkless.Services.ActionBuilders.EngageActions;
using Quarkless.Services.Interfaces;
using QuarklessContexts.Models.Profiles;

namespace Quarkless.Services.Factories.Actions
{
	public class CreateCommentMediaActionBuilderFactory : ActionBuilderFactory
	{
		public override IActionCommit Commit(IContentManager builder, ProfileModel profile)
			=> new CreateCommentAction(builder,profile);
	}
}