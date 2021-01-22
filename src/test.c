
int main() {
	int a = 1 + 5 + 12 * 3;
	int b = a;

	a = (a + 2 + a) + 5;

	printf(a);
	printf(b);
	return 0;
}