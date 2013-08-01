// Managed C++.h

#pragma once


#include <msclr\marshal_cppstd.h>

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

      int Count(String^ str)
      {
         std::string stdString = msclr::interop::marshal_as< std::string >( str);
         return stdString.length();
      }
	};
}
