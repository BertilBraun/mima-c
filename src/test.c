
/* int a = 5, b, c = 5; */
/* double b; */

/* void a(int a, int b); */
/* void b(); */

/* int a(int b, int c) { */
/*     c = 1 / 2 * 3 + 2 + c; */
/*     b = c = 2 + 1 - 2; */
/* } */

int fac(int n) {
    if (n == 1) return 1;
    return n * fac(n - 1);
}

int fibbonachi(int n) {
    int a = 0, b = 1;
    for (int i = 0; i < n; i = i + 1) {
        int c = b;
        b = a + b;
        a = c;
    }
    return a;
}

// TODO: Make this the first goal,
//       to be able to properly run this function


int main() {
    for (int i = 0; i < 20; i = i + 1) {
        if (i % 2 == 0)
            continue;
        if (i == 17)
            break;
        printf("{}", fibbonachi(i));
    }
    printf("Done");
    return 0;
}
