// Managed C++.h

#pragma once

using namespace System;

namespace ManagedCPP {

	public ref class ManagedClass
	{

   private:
      int x;
   public:
      ManagedClass()
      {
         x = 5;
      }

      static int add(int a, int b)
      {
         return a + b;
      }

      int getX()
      {
         return x;
      }
	};
}
