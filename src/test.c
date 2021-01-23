
int foo() {
	int a = 2 + 5;
	int b = a + 7;
	return b + 2;
}

int bar() {
	int a = foo();
	int b = a + 42 + foo();
	int d = 27 + 3;
	int e = b;
	printf(a, b, d, e, foo());
	return a;
}

int main() {
	int test = foo();
	int a = 1;
	int b = a + 2;
	int c = b + 3;
	int test2 = test + foo();
	int d = c + 4;
	int e = d + 5;
	printf(a, b, c, d, e);
	return a;
}