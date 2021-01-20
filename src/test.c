
struct S {
    int a;
    int* b;
};

typedef int bool;
typedef bool bool;

typedef S Struct;

typedef struct {
    bool b = 2;
} W;

typedef struct X {

} X;

int main() {

    bool f = 0;
    // int a = (int)b;

    int b = 3;

    S test1;
    Struct test2;
    test1.a = 2;
    test1.b = &b;
    printf(test1.a);
    printf(*test1.b);
    W test3;

    // if (b == 0)
    //     printf(b);
    printf("Done");
    return 0;
}
