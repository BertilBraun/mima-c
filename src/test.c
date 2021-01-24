
int foo() {
	int a = 2 + 5;
	int b = a + 7;
	return b + 2;
}

int main() {

	int a = 1;
	int b = a + 2;

	int test1 = b + foo();
	int test2 = b + foo();
	int test3 = test1 + foo();

	int c = b + 3;
	int d = c + 4;
	int e = d + 5;

	int delim = 420;

	printf(a, b, c, d, e);
	printf(delim);
	printf(test1, test2, test3);
	return a;
}