
/* int a = 5, b, c = 5; */
/* double b; */

/* void a(int a, int b); */
/* void b(); */

/* int a(int b, int c) { */
/*     c = 1 / 2 * 3 + 2 + c; */
/*     b = c = 2 + 1 - 2; */
/* } */


//int fac(int n) {
//    if (n == 1) return 1;
//    return n * fac(n - 1);
//}


// int a, b[1];

// This was the first goal,
// to be able to properly run this function
//int fibbonachi(int n) {
//    int a = 0, b = 1;
//    for (int i = 0; i < n; i = i + 1) {
//        int c = b;
//        b = a + b;
//        a = c;
//    }
//    return a;
//}

//void print(int a, int b, int c) {
//    printf("{0}, {1}, {2}", a, b, c);
//}


//int main() {
//    for (int i = 0; i < 20; i = i + 1) {
//        if (i % 2 == 0)
//            continue;
//        if (i == 17)
//            break;
//        printf(fibbonachi(i));
//    }
//    print(1, 2, 3);
//    printf("Done");
//    return 0;
//}

int test() {
    return 42;
}

int main() {
    int f[5] = { 1, test(), 3, 4, 5 };
    for (int i = 0; i != 5; i += 1) {
        if (i == 0 && 1 == 1 || 2 - 2 == 0)
            printf("Jay");
        f[i] = f[i] + 1;
    }
    for (int i = 0; i < 5; i = i + 1) {
        printf(f[i]);
    }
    printf("Done");
    return 0;
}