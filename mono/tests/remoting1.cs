using System;
using System.Threading;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

class MyProxy : RealProxy {
	readonly MarshalByRefObject target;

	public MyProxy (MarshalByRefObject target) : base (target.GetType())
	{
		this.target = target;
	}

	public override IMessage Invoke (IMessage request) {
		IMethodCallMessage call = (IMethodCallMessage)request;
		Console.WriteLine ("Invoke " + call.MethodName);

		Console.Write ("ARGS(");
		for (int i = 0; i < call.ArgCount; i++) {
			if (i != 0)
				Console.Write (", ");
			Console.Write (call.GetArgName (i) +  " " +
				       call.GetArg (i));
		}
		Console.WriteLine (")");
		Console.Write ("INARGS(");
		for (int i = 0; i < call.InArgCount; i++) {
			if (i != 0)
				Console.Write (", ");
			Console.Write (call.GetInArgName (i) +  " " +
				       call.GetInArg (i));
		}
		Console.WriteLine (")");

		IMethodReturnMessage res = RemotingServices.ExecuteMessage (target, call);

		Console.Write ("RESARGS(");
		for (int i = 0; i < res.ArgCount; i++) {
			if (i != 0)
				Console.Write (", ");
			Console.Write (res.GetArgName (i) +  " " +
				       res.GetArg (i));
		}
		Console.WriteLine (")");		
		
		Console.Write ("RESOUTARGS(");
		for (int i = 0; i < res.OutArgCount; i++) {
			if (i != 0)
				Console.Write (", ");
			Console.Write (res.GetOutArgName (i) +  " " +
				       res.GetOutArg (i));
		}
		Console.WriteLine (")");		
		
		return res;
	}
}

public struct MyStruct {
	public int a;
	public int b;
	public int c;
}
	
class R1 : MarshalByRefObject {

	public int test_field = 5;
	
	public virtual MyStruct Add (int a, out int c, int b) {
		Console.WriteLine ("ADD");
		c = a + b;

		MyStruct res = new MyStruct ();

		res.a = a;
		res.b = b;
		res.c = c;
		
		return res;
	}

	public long nonvirtual_Add (int a, int b) {
		Console.WriteLine ("nonvirtual_Add");
		return a + b;
	}
}

class Test {

	static long test_call (R1 o)
	{
		return o.nonvirtual_Add (2, 3);
	}
	
	static int Main () {
		R1 myobj = new R1 ();
		int res = 0;
		long lres;
		
		MyProxy real_proxy = new MyProxy (myobj);

		R1 o = (R1)real_proxy.GetTransparentProxy ();

		if (RemotingServices.IsTransparentProxy (null))
			return 1;
		
		if (!RemotingServices.IsTransparentProxy (o))
			return 1;

		Console.WriteLine (o.GetType ());
		
		MyStruct myres = o.Add (2, out res, 3);

		Console.WriteLine ("Result: " + myres.a + " " +
				   myres.b + " " + myres.c +  " " + res);

		if (myres.a != 2)
			return 1;
		
		if (myres.b != 3)
			return 1;
		
		if (myres.c != 5)
			return 1;

		if (res != 5)
			return 1;

		R1 o2 = new R1 ();
		
		lres = test_call (o2);
		
		lres = test_call (o);

		Console.WriteLine ("Result: " + lres);
		if (lres != 5)
			return 1;
		
		lres = test_call (o);

		o.test_field = 2;
		
		Console.WriteLine ("test_field: " + o.test_field);
		if (o.test_field != 2)
			return 1;

		return 0;
	}
}
