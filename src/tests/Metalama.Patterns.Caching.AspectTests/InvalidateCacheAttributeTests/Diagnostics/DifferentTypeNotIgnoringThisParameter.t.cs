// Error LAMA5101 on `Invalidate`: `The [InvalidateCache] aspect applied to 'DifferentTypeNotIgnoringThisParameter.InvalidatingClass.Invalidate()' cannot invalidate 'DifferentTypeNotIgnoringThisParameter.CachingClass.DoAction()': the 'this' parameter cannot be mapped because (a) 'DifferentTypeNotIgnoringThisParameter.CachingClass.DoAction()' is an instance method, (b) the IgnoreThisParameter is not enabled and (c) the type DifferentTypeNotIgnoringThisParameter.InvalidatingClass is either static or not derived from 'DifferentTypeNotIgnoringThisParameter.CachingClass'.`